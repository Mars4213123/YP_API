using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

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

        public class RecipeStep
        {
            public int StepNumber { get; set; }
            public string StepText { get; set; }
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
            RecipeImage.Source = new BitmapImage(new Uri(imageUrl));

            IngredientsList.ItemsSource = ingredients;

            var stepObjects = new List<RecipeStep>();
            int i = 1;
            foreach (var step in steps)
                stepObjects.Add(new RecipeStep { StepNumber = i++, StepText = step });
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
            MessageBox.Show("Ингредиенты добавлены в список покупок.", "Список покупок",
                            MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddToFavorites_Click(object sender, RoutedEventArgs e)
        {
            if (!AppData.Favorites.Any(f => f.Title == _currentRecipe.Title))
            {
                AppData.Favorites.Add(_currentRecipe);
                MessageBox.Show("Рецепт добавлен в избранное!", "Избранное",
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Этот рецепт уже в избранном.", "Избранное",
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


    }
}

