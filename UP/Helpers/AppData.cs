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
        public static ApiService ApiService { get; set; } = new ApiService();
        public static UserData CurrentUser { get; set; }

        public static ObservableCollection<string> Products { get; set; } = new ObservableCollection<string>();
        public static ObservableCollection<Receipts.DailyMenu> WeeklyMenu { get; set; } = new ObservableCollection<Receipts.DailyMenu>();
        public static ObservableCollection<string> ShoppingList { get; set; } = new ObservableCollection<string>();
        public static ObservableCollection<RecipeDetailsPage.RecipeData> Favorites { get; set; } = new ObservableCollection<RecipeDetailsPage.RecipeData>();
        public static ObservableCollection<RecipeDto> AllRecipes { get; set; } = new ObservableCollection<RecipeDto>();

        public static async void InitializeAfterLogin(UserData user)
        {
            CurrentUser = user;
            await LoadInitialData();
        }

        public static async Task LoadInitialData()
        {
            try
            {
                await LoadRecipes();
                await LoadFavorites();
                await LoadCurrentMenu();
                await LoadShoppingList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка");
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
                                Description = $"Калории: {meal.Calories} | Время: {meal.PrepTime} мин"
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
                            ShoppingList.Add($"{item.IngredientName} - {item.Quantity} {item.Unit}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading shopping list: {ex.Message}");
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

        public static async Task<bool> GenerateNewMenu(GenerateMenuRequest request)
        {
            try
            {
                Console.WriteLine($"Generating menu with request: Days={request.Days}, Calories={request.TargetCaloriesPerDay}, Tags={string.Join(", ", request.CuisineTags)}");

                var menu = await ApiService.GenerateMenuAsync(request);

                Console.WriteLine($"Menu generation response: {(menu != null ? "Success" : "Null response")}");

                if (menu != null)
                {
                    Console.WriteLine($"Menu ID: {menu.Id}, Days count: {menu.Days?.Count}");

                    if (menu.Days != null && menu.Days.Count > 0)
                    {
                        await LoadCurrentMenu();

                        if (menu.Id > 0)
                        {
                            Console.WriteLine($"Generating shopping list for menu ID: {menu.Id}");
                            await ApiService.GenerateShoppingListAsync(menu.Id);
                            await LoadShoppingList();
                        }
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("Menu generated but has no days");
                        return false;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating menu: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        private static RecipeDetailsPage.RecipeData ConvertToRecipeData(RecipeDto recipe)
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