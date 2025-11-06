using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using UP.Models;
using UP.Services;

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

        private RecipeData _currentRecipe;

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

            RecipeTitleText.Text = title;
            RecipeDescriptionText.Text = description;

            if (!string.IsNullOrEmpty(imageUrl))
            {
                RecipeImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(imageUrl));
            }

            IngredientsList.ItemsSource = ingredients;

            var stepObjects = new List<object>();
            int i = 1;
            foreach (var step in steps)
                stepObjects.Add(new { StepNumber = i++, StepText = step });
            StepsList.ItemsSource = stepObjects;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.mainWindow.OpenPages(new Receipts());
        }

        private void StartTimer_Click(object sender, RoutedEventArgs e)
        {
            var timerWindow = new TimerWindow(RecipeTitleText.Text);
            timerWindow.ShowDialog();
        }

        private void AddToShoppingList_Click(object sender, RoutedEventArgs e)
        {
            // Добавляем ингредиенты в локальный список
            foreach (var ingredient in _currentRecipe.Ingredients)
            {
                AppData.ShoppingList.Add(ingredient);
            }

            MessageBox.Show("Ингредиенты добавлены в список покупок.", "Список покупок",
                            MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void AddToFavorites_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var success = await AppData.AddToFavorites(_currentRecipe);
                if (success)
                {
                    // Проверяем, нет ли уже в избранном
                    if (!AppData.Favorites.Any(f => f.Title == _currentRecipe.Title))
                    {
                        AppData.Favorites.Add(_currentRecipe);
                    }

                    MessageBox.Show("Рецепт добавлен в избранное!", "Избранное",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Не удалось добавить в избранное", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}