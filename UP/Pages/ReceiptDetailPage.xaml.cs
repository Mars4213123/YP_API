using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using UP.Models;

namespace UP.Pages
{
    public partial class RecipeDetailsPage : Page
    {
        public class RecipeData
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string ImageUrl { get; set; }
            public List<string> Ingredients { get; set; }
            public List<string> Steps { get; set; }
        }

        public class IngredientDisplay
        {
            public string Name { get; set; }
            public decimal Quantity { get; set; }
            public string Unit { get; set; }
            
            public override string ToString()
            {
                if (Quantity > 0)
                    return $"• {Name} - {Quantity} {Unit}";
                return $"• {Name}";
            }
        }

        private RecipeData _currentRecipe;
        private List<IngredientDisplay> _ingredientsList = new List<IngredientDisplay>();

        public RecipeDetailsPage(string title, string description, string imageUrl, List<string> ingredients, List<string> steps)
        {
            InitializeComponent();

            _currentRecipe = new RecipeData
            {
                Title = title,
                Description = description,
                ImageUrl = imageUrl,
                Ingredients = ingredients,
                Steps = steps
            };

            LoadRecipeData();
        }

        public RecipeDetailsPage(RecipeDto recipe)
        {
            InitializeComponent();

            Console.WriteLine($"[RecipeDetailsPage] Загружаем рецепт: {recipe.Title}");
            Console.WriteLine($"[RecipeDetailsPage] Ингредиентов: {recipe.Ingredients?.Count ?? 0}");
            Console.WriteLine($"[RecipeDetailsPage] Шаги: {recipe.Instructions?.Count ?? 0}");

            _currentRecipe = new RecipeData
            {
                Title = recipe.Title ?? "Рецепт",
                Description = recipe.Description ?? "",
                ImageUrl = recipe.ImageUrl ?? "",
                Ingredients = new List<string>(),
                Steps = recipe.Instructions ?? new List<string>()
            };

            // Преобразуем ингредиенты для отображения
            if (recipe.Ingredients != null && recipe.Ingredients.Count > 0)
            {
                foreach (var ingredient in recipe.Ingredients)
                {
                    if (ingredient != null)
                    {
                        _ingredientsList.Add(new IngredientDisplay
                        {
                            Name = ingredient.Name ?? "Неизвестный ингредиент",
                            //Quantity = ingredient.Quantity,
                            Unit = ingredient.Unit ?? "шт"
                        });
                    }
                }
            }

            LoadRecipeData();
        }

        private void LoadRecipeData()
        {
            try
            {
                RecipeTitleText.Text = _currentRecipe.Title;
                RecipeDescriptionText.Text = _currentRecipe.Description;

                // Загружаем изображение
                LoadImage(_currentRecipe.ImageUrl);

                // Используем список ингредиентов
                if (_ingredientsList.Count > 0)
                {
                    IngredientsList.ItemsSource = _ingredientsList;
                    Console.WriteLine($"[LoadRecipeData] Загружено {_ingredientsList.Count} ингредиентов");
                }
                else if (_currentRecipe.Ingredients.Count > 0)
                {
                    IngredientsList.ItemsSource = _currentRecipe.Ingredients;
                    Console.WriteLine($"[LoadRecipeData] Загружено {_currentRecipe.Ingredients.Count} ингредиентов (строки)");
                }
                else
                {
                    IngredientsList.ItemsSource = new List<string> { "Нет информации об ингредиентах" };
                }

                // Загружаем шаги приготовления
                var stepObjects = new ObservableCollection<dynamic>();
                int stepNumber = 1;
                
                if (_currentRecipe.Steps != null && _currentRecipe.Steps.Count > 0)
                {
                    foreach (var step in _currentRecipe.Steps)
                    {
                        if (!string.IsNullOrWhiteSpace(step))
                        {
                            stepObjects.Add(new { StepNumber = stepNumber++, StepText = step });
                        }
                    }
                }
                
                if (stepObjects.Count == 0)
                {
                    stepObjects.Add(new { StepNumber = 1, StepText = "Инструкции не доступны" });
                }
                
                StepsList.ItemsSource = stepObjects;
                Console.WriteLine($"[LoadRecipeData] Загружено {stepObjects.Count} шагов приготовления");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoadRecipeData] Ошибка: {ex.Message}");
                MessageBox.Show($"Ошибка при загрузке рецепта: {ex.Message}", "Ошибка");
            }
        }

        private void LoadImage(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                Console.WriteLine("[LoadImage] URL картинки пуст, используем заглушку");
                SetDefaultImage();
                return;
            }

            try
            {
                Console.WriteLine($"[LoadImage] Загружаем изображение: {imageUrl}");
                
                // Пытаемся загрузить изображение
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imageUrl, UriKind.RelativeOrAbsolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                
                bitmap.DownloadCompleted += (s, e) => Console.WriteLine("[LoadImage] Изображение успешно загружено");
                bitmap.DownloadFailed += (s, e) => 
                {
                    Console.WriteLine($"[LoadImage] Ошибка загрузки: {e.ErrorException?.Message}");
                    SetDefaultImage();
                };

                RecipeImage.Source = bitmap;
                RecipeImage.Height = 250;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoadImage] Exception: {ex.Message}");
                SetDefaultImage();
            }
        }

        private void SetDefaultImage()
        {
            try
            {
                // Используем цветную заглушку
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri("pack://application:,,,/Resources/DefaultRecipe.png", UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                
                RecipeImage.Source = bitmap;
            }
            catch
            {
                // Если нет файла, просто скрываем
                RecipeImage.Visibility = Visibility.Collapsed;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.mainWindow.OpenPages(new Receipts());
        }

        private void AddToShoppingList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int addedCount = 0;
                foreach (var ingredient in _ingredientsList)
                {
                    // ShoppingList хранит строки, добавляем в формате "Название - Количество Единица"
                    string itemText = $"{ingredient.Name}";
                    if (ingredient.Quantity > 0)
                    {
                        itemText += $" - {ingredient.Quantity} {ingredient.Unit}";
                    }
                    
                    AppData.ShoppingList.Add(itemText);
                    addedCount++;
                }

                if (addedCount > 0)
                {
                    MessageBox.Show(
                        $"Добавлено {addedCount} ингредиентов в список покупок!",
                        "Успех",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Нечего добавлять", "Информация");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AddToShoppingList_Click] Error: {ex.Message}");
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
            }
        }

        private async void AddToFavorites_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine($"[AddToFavorites_Click] Добавляем в избранное: {_currentRecipe.Title}");
                
                // Преобразуем в RecipeData формат которы ожидает AddToFavorites
                var recipeDataForFav = new RecipeData
                {
                    Title = _currentRecipe.Title,
                    Description = _currentRecipe.Description,
                    ImageUrl = _currentRecipe.ImageUrl,
                    Ingredients = _ingredientsList.Select(i => $"{i.Name} - {i.Quantity} {i.Unit}").ToList(),
                    Steps = _currentRecipe.Steps
                };

                var success = await AppData.AddToFavorites(recipeDataForFav);
                
                if (success)
                {
                    MessageBox.Show(
                        "Рецепт добавлен в избранное!",
                        "Успех",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Не удалось добавить в избранное", "Ошибка");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AddToFavorites_Click] Error: {ex.Message}");
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
            }
        }
    }
}