using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using UP.Models;
using UP.Pages;
using UP.Services;

namespace UP
{
    public static class AppData
    {
        public static ApiService ApiService { get; set; }
        public static UserData CurrentUser { get; set; }

        public static ObservableCollection<string> Products { get; } = new ObservableCollection<string>();
        public static ObservableCollection<Receipts.DailyMenu> WeeklyMenu { get; } = new ObservableCollection<Receipts.DailyMenu>();
        public static ObservableCollection<string> ShoppingList { get; } = new ObservableCollection<string>();
        public static ObservableCollection<RecipeDetailsPage.RecipeData> Favorites { get; } = new ObservableCollection<RecipeDetailsPage.RecipeData>();
        public static ObservableCollection<Models.RecipeDto> AllRecipes { get; } = new ObservableCollection<Models.RecipeDto>();
        public static List<RecipeDto> FridgeRecipes { get; set; }


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
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка");
                return false;
            }
        }

        public static async Task LoadRecipes()
        {
            try
            {
                var recipes = await ApiService.GetRecipesAsync();
                AllRecipes.Clear();

                foreach (var recipe in recipes)
                {
                    AllRecipes.Add(recipe);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading recipes: {ex.Message}");
            }
        }

        public static async Task LoadFavorites()
        {
            try
            {
                var favorites = await ApiService.GetFavoritesAsync();
                Favorites.Clear();

                foreach (var recipe in favorites)
                {
                    Favorites.Add(ConvertToRecipeData(recipe));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading favorites: {ex.Message}");
            }
        }

        public static async Task LoadCurrentMenu()
        {
            try
            {
                var menu = await ApiService.GetCurrentMenuAsync();
                WeeklyMenu.Clear();

                if (menu != null && menu.Days != null)
                {
                    foreach (var day in menu.Days)
                    {
                        foreach (var meal in day.Meals)
                        {
                            WeeklyMenu.Add(new Receipts.DailyMenu
                            {
                                Day = day.Date,
                                Meal = meal.RecipeTitle,
                                Description = $"Калории: {meal.Calories} | Время: {meal.PrepTime} мин",
                                RecipeId = meal.RecipeId
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading menu: {ex.Message}");
            }
        }

        public static async Task LoadShoppingList()
        {
            try
            {
                var shoppingList = await ApiService.GetCurrentShoppingListAsync();
                ShoppingList.Clear();

                if (shoppingList != null && shoppingList.Items != null)
                {
                    foreach (var item in shoppingList.Items)
                    {
                        if (!item.IsPurchased)
                        {
                            ShoppingList.Add($"{item.Name} - {item.Quantity} {item.Unit}");
                        }
                    }

                    Console.WriteLine($"Загружено {shoppingList.Items.Count} товаров в список покупок");
                }
                else
                {
                    Console.WriteLine("Список покупок пуст или не загружен");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading shopping list: {ex.Message}");
                MessageBox.Show($"Ошибка загрузки списка покупок: {ex.Message}", "Ошибка");
            }
        }

        public static async Task<bool> AddToFavorites(RecipeDetailsPage.RecipeData recipe)
        {
            try
            {
                var apiRecipe = AllRecipes.FirstOrDefault(r => r.Title == recipe.Title);
                if (apiRecipe != null)
                {
                    return await ApiService.ToggleFavoriteAsync(apiRecipe.Id);
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding to favorites: {ex.Message}");
                return false;
            }
        }
        public static async Task LoadFridgeRecipes()
        {
            var userId = CurrentUser?.Id ?? 0;
            if (userId == 0)
            {
                FridgeRecipes = new List<RecipeDto>();
                return;
            }

            FridgeRecipes = await ApiService.GetRecipesByFridgeAsync(userId);
        }

        public static async Task<bool> RemoveFromFavorites(RecipeDetailsPage.RecipeData recipe)
        {
            try
            {
                var apiRecipe = AllRecipes.FirstOrDefault(r => r.Title == recipe.Title);
                if (apiRecipe != null)
                {
                    return await ApiService.ToggleFavoriteAsync(apiRecipe.Id);
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing from favorites: {ex.Message}");
                return false;
            }
        }

        private static RecipeDetailsPage.RecipeData ConvertToRecipeData(Models.RecipeDto recipe)
        {
            return new RecipeDetailsPage.RecipeData
            {
                Title = recipe.Title,
                Description = recipe.Description,
                ImageUrl = recipe.ImageUrl,
                Ingredients = recipe.Ingredients?.ConvertAll(i => $"{i.Name} - {i.Quantity} {i.Unit}") ?? new List<string>(),
                Steps = recipe.Instructions ?? new List<string>()
            };
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
        }
    }
}