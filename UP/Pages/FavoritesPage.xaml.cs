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
            LoadFavorites();
        }

        private void LoadFavorites()
        {
            FavoritesList.ItemsSource = AppData.Favorites;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.mainWindow.OpenPages(new Receipts());
        }

        private async void OpenRecipe_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is RecipeDetailsPage.RecipeData recipe)
            {
                var apiRecipe = AppData.AllRecipes.FirstOrDefault(r => r.Title == recipe.Title);

                if (apiRecipe != null)
                {
                    var detailsPage = new RecipeDetailsPage(apiRecipe);
                    MainWindow.mainWindow.OpenPages(detailsPage);
                }
                else
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
        }

        private async void RemoveRecipe_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is RecipeDetailsPage.RecipeData recipe)
            {
                if (MessageBox.Show($"Удалить рецепт '{recipe.Title}' из избранного?", "Подтверждение",
                                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    var success = await AppData.RemoveFromFavorites(recipe);
                    if (success)
                    {
                        AppData.Favorites.Remove(recipe);
                        MessageBox.Show("Рецепт удален из избранного", "Успех");
                    }
                    else
                    {
                        MessageBox.Show("Не удалось удалить рецепт", "Ошибка");
                    }
                }
            }
        }

        private async void RefreshFavorites_Click(object sender, RoutedEventArgs e)
        {
            await AppData.LoadFavorites();
            LoadFavorites();
        }
    }
}