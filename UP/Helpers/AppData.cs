using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using UP.Models;
using UP.Services;

namespace UP
{
    // Вспомогательный класс для отображения в списке меню
    public class DailyMenuViewModel
    {
        public string Day { get; set; }
        public string Meal { get; set; }
        public string Description { get; set; }
        public int RecipeId { get; set; }
    }

    public static class AppData
    {
        public static ApiService ApiService { get; set; }
        public static UserData CurrentUser { get; set; }

        public static ObservableCollection<IngredientDto> Products { get; } = new ObservableCollection<IngredientDto>();

        public static ObservableCollection<DailyMenuViewModel> WeeklyMenu { get; } = new ObservableCollection<DailyMenuViewModel>();

        public static ObservableCollection<string> ShoppingList { get; } = new ObservableCollection<string>();

        // ИСПРАВЛЕНО: Теперь здесь RecipeDto, а не RecipeData
        public static ObservableCollection<RecipeDto> Favorites { get; } = new ObservableCollection<RecipeDto>();

        public static ObservableCollection<RecipeDto> AllRecipes { get; } = new ObservableCollection<RecipeDto>();

        public static List<RecipeDto> FridgeRecipes { get; set; } = new List<RecipeDto>();

        public static async Task<bool> InitializeAfterLogin(UserData user)
        {
            CurrentUser = user;
            return await LoadInitialData();
        }

        public static async Task<bool> LoadInitialData()
        {
            try
            {
                await LoadRecipes();
                await LoadFavorites();
                await LoadCurrentMenu();
                await LoadShoppingList();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации: {ex.Message}");
                return false;
            }
        }

        public static async Task LoadRecipes()
        {
            try
            {
                var recipes = await ApiService.GetRecipesAsync();
                AllRecipes.Clear();
                foreach (var r in recipes) AllRecipes.Add(r);
            }
            catch { }
        }

        public static async Task LoadFavorites()
        {
            try
            {
                var favs = await ApiService.GetFavoritesAsync();
                Favorites.Clear();
                // ИСПРАВЛЕНО: Просто добавляем DTO, конвертация не нужна
                foreach (var r in favs) Favorites.Add(r);
            }
            catch { }
        }

        public static async Task LoadCurrentMenu()
        {
            try
            {
                var menu = await ApiService.GetCurrentMenuAsync();
                WeeklyMenu.Clear();
                if (menu?.Days != null)
                {
                    foreach (var day in menu.Days)
                    {
                        foreach (var meal in day.Meals)
                        {
                            WeeklyMenu.Add(new DailyMenuViewModel
                            {
                                Day = day.Date,
                                Meal = meal.MealType,
                                Description = meal.RecipeTitle,
                                RecipeId = meal.RecipeId
                            });
                        }
                    }
                }
            }
            catch { }
        }

        public static async Task LoadShoppingList()
        {
            try
            {
                var list = await ApiService.GetCurrentShoppingListAsync();
                ShoppingList.Clear();
                if (list?.Items != null)
                {
                    foreach (var item in list.Items)
                    {
                        if (!item.IsPurchased)
                            ShoppingList.Add($"{item.Name} - {item.Quantity} {item.Unit}");
                    }
                }
            }
            catch { }
        }

        // ИСПРАВЛЕНО: Принимаем RecipeDto
        public static async Task<bool> AddToFavorites(RecipeDto recipe)
        {
            if (recipe == null) return false;
            var success = await ApiService.ToggleFavoriteAsync(recipe.Id);
            if (success && !Favorites.Any(f => f.Id == recipe.Id))
            {
                Favorites.Add(recipe);
            }
            return success;
        }

        // ИСПРАВЛЕНО: Принимаем RecipeDto
        public static async Task<bool> RemoveFromFavorites(RecipeDto recipe)
        {
            if (recipe == null) return false;
            var success = await ApiService.ToggleFavoriteAsync(recipe.Id);
            if (success)
            {
                var item = Favorites.FirstOrDefault(f => f.Id == recipe.Id);
                if (item != null) Favorites.Remove(item);
            }
            return success;
        }

        public static async Task LoadFridgeRecipes()
        {
            if (CurrentUser == null) return;
            FridgeRecipes = await ApiService.GetRecipesByFridgeAsync(CurrentUser.Id);
        }

        public static void Logout()
        {
            CurrentUser = null;
            ApiService.ClearToken();
            Products.Clear();
            WeeklyMenu.Clear();
            ShoppingList.Clear();
            Favorites.Clear();
            AllRecipes.Clear();
            FridgeRecipes?.Clear();
        }
    }
}