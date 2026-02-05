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

        public static ObservableCollection<Models.IngredientDto> Products { get; } = new ObservableCollection<Models.IngredientDto>();
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

                if (menu != null && menu.Items != null)
                {
                    // Группируем элементы меню по дате
                    var groupedItems = menu.Items
                        .GroupBy(i => i.Date) // Группируем по строке даты "yyyy-MM-dd"
                        .OrderBy(g => g.Key); // Сортируем по дате

                    foreach (var dayGroup in groupedItems)
                    {
                        foreach (var meal in dayGroup)
                        {
                            WeeklyMenu.Add(new Receipts.DailyMenu
                            {
                                Day = dayGroup.Key.ToString("dd.MM.yyyy"), // Преобразуем дату в строку
                                                                           // Дата (строка)
                                Meal = meal.RecipeTitle,
                                Description = $"{meal.MealType}", // Или другое описание
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
                Console.WriteLine($"[AddToFavorites] Добавляем в избранное: {recipe.Title}");
                
                var apiRecipe = AllRecipes.FirstOrDefault(r => r.Title == recipe.Title);
                if (apiRecipe != null)
                {
                    var success = await ApiService.ToggleFavoriteAsync(apiRecipe.Id);
                    
                    if (success)
                    {
                        Console.WriteLine($"[AddToFavorites] Успешно добавлено на сервер, добавляем в локальный список");
                        
                        // Добавляем в локальный список если его там еще нет
                        if (!Favorites.Any(f => f.Title == recipe.Title))
                        {
                            Favorites.Add(recipe);
                            Console.WriteLine($"[AddToFavorites] Рецепт добавлен в локальный список. Всего: {Favorites.Count}");
                        }
                        else
                        {
                            Console.WriteLine($"[AddToFavorites] Рецепт уже в списке");
                        }
                    }
                    
                    return success;
                }
                
                Console.WriteLine($"[AddToFavorites] Рецепт не найден в AllRecipes");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AddToFavorites] Exception: {ex.Message}");
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
                Console.WriteLine($"[RemoveFromFavorites] Удаляем из избранного: {recipe.Title}");
                
                var apiRecipe = AllRecipes.FirstOrDefault(r => r.Title == recipe.Title);
                if (apiRecipe != null)
                {
                    var success = await ApiService.ToggleFavoriteAsync(apiRecipe.Id);
                    
                    if (success)
                    {
                        Console.WriteLine($"[RemoveFromFavorites] Успешно удалено на сервере");
                        return true;
                    }
                    
                    return false;
                }
                
                Console.WriteLine($"[RemoveFromFavorites] Рецепт не найден в AllRecipes");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RemoveFromFavorites] Exception: {ex.Message}");
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
                //Ingredients = recipe.Ingredients?.ConvertAll(i => $"{i.Name} - {i.Quantity} {i.Unit}") ?? new List<string>(),
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