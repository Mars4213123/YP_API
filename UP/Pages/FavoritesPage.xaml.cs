using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace UP.Pages
{
    public partial class FavoritesPage : Page
    {
        public FavoritesPage()
        {
            InitializeComponent();
            FavoritesList.ItemsSource = AppData.Favorites;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.mainWindow.OpenPages(new Receipts());
        }

        private void OpenRecipe_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is RecipeDetailsPage.RecipeData recipe)
            {
                var detailsPage = new RecipeDetailsPage(
                    recipe.Title,
                    recipe.Description,
                    recipe.ImageUrl,
                    recipe.Ingredients,
                    recipe.Steps
                );

                MainWindow.mainWindow.OpenPages(detailsPage);
            }
        }

        private void RemoveRecipe_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is RecipeDetailsPage.RecipeData recipe)
            {
                if (MessageBox.Show($"Удалить рецепт '{recipe.Title}' из избранного?", "Подтверждение",
                                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    AppData.Favorites.Remove(recipe);
                }
            }
        }
    }
}
