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
        // Вспомогательный класс для отображения ингредиентов в ListView
        public class IngredientViewItem
        {
            public string Text { get; set; }
        }

        // Вспомогательный класс для отображения шагов
        public class StepViewItem
        {
            public int Number { get; set; }
            public string Text { get; set; }
        }

        private RecipeDto _currentRecipe;
        private int _recipeId;

        // Конструктор по ID (основной)
        public RecipeDetailsPage(int recipeId)
        {
            InitializeComponent();
            _recipeId = recipeId;
            Loaded += Page_Loaded;
        }

        // Конструктор по объекту (вспомогательный)
        public RecipeDetailsPage(RecipeDto recipe)
        {
            InitializeComponent();
            _currentRecipe = recipe;
            _recipeId = recipe.Id;
            DisplayRecipe(_currentRecipe);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_currentRecipe == null && _recipeId > 0)
            {
                try
                {
                    // Загружаем рецепт через API
                    var recipe = await AppData.ApiService.GetRecipeAsync(_recipeId);
                    if (recipe != null)
                    {
                        _currentRecipe = recipe;
                        DisplayRecipe(_currentRecipe);
                    }
                    else
                    {
                        MessageBox.Show("Рецепт не найден.", "Ошибка");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка");
                }
            }
        }

        private void DisplayRecipe(RecipeDto recipe)
        {
            if (recipe == null) return;

            RecipeTitleText.Text = recipe.Title;
            RecipeDescriptionText.Text = recipe.Description;

            // Загрузка картинки
            LoadImage(recipe.ImageUrl);

            // Ингредиенты
            if (recipe.Ingredients != null)
            {
                var ingredientItems = recipe.Ingredients.Select(i => new IngredientViewItem
                {
                    Text = $"• {i.Name} {i.Unit}" // Можно добавить количество, если оно есть в DTO
                }).ToList();

                if (ingredientItems.Count == 0)
                    ingredientItems.Add(new IngredientViewItem { Text = "Нет информации об ингредиентах" });

                IngredientsList.ItemsSource = ingredientItems;
            }

            // Шаги приготовления
            if (recipe.Instructions != null && recipe.Instructions.Any())
            {
                var steps = new List<StepViewItem>();
                int i = 1;
                foreach (var step in recipe.Instructions)
                {
                    steps.Add(new StepViewItem { Number = i++, Text = step });
                }
                StepsList.ItemsSource = steps;
            }
            else
            {
                StepsList.ItemsSource = new List<StepViewItem> { new StepViewItem { Number = 1, Text = "Инструкции не доступны" } };
            }
        }

        private void LoadImage(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                SetDefaultImage();
                return;
            }

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imageUrl, UriKind.RelativeOrAbsolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                bitmap.DownloadFailed += (s, e) => SetDefaultImage();
                RecipeImage.Source = bitmap;
            }
            catch
            {
                SetDefaultImage();
            }
        }

        private void SetDefaultImage()
        {
            try
            {
                // Укажите правильный путь к заглушке в ресурсах
                var bitmap = new BitmapImage(new Uri("pack://application:,,,/Resources/DefaultRecipe.png"));
                RecipeImage.Source = bitmap;
            }
            catch
            {
                RecipeImage.Visibility = Visibility.Collapsed;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack) NavigationService.GoBack();
        }

        private void AddToShoppingList_Click(object sender, RoutedEventArgs e)
        {
            if (_currentRecipe?.Ingredients == null) return;

            try
            {
                int count = 0;
                foreach (var ing in _currentRecipe.Ingredients)
                {
                    // Добавляем в глобальный список покупок (строки)
                    string itemText = $"{ing.Name} {ing.Unit}";
                    AppData.ShoppingList.Add(itemText);
                    count++;
                }

                if (count > 0)
                    MessageBox.Show($"Добавлено {count} ингредиентов.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show("Нечего добавлять.", "Инфо");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
            }
        }

        private async void AddToFavorites_Click(object sender, RoutedEventArgs e)
        {
            if (_currentRecipe == null) return;

            try
            {
                // Теперь передаем RecipeDto, как требует обновленный AppData
                bool success = await AppData.AddToFavorites(_currentRecipe);

                if (success)
                    MessageBox.Show("Рецепт добавлен в избранное!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show("Уже в избранном или ошибка.", "Инфо");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
            }
        }
    }
}