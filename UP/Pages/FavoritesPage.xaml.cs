using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using UP.Models; // Важно: Используем RecipeDto
using UP.Pages;

namespace UP.Pages
{
    public partial class FavoritesPage : Page
    {
        private ObservableCollection<RecipeDto> _favorites;

        public FavoritesPage()
        {
            InitializeComponent();
            _favorites = AppData.Favorites;
            LoadFavorites();
        }

        private void LoadFavorites()
        {
            try
            {
                FavoritesList.ItemsSource = _favorites;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки избранного: {ex.Message}", "Ошибка");
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.mainWindow.OpenPages(new Receipts());
        }

        private void OpenRecipe_Click(object sender, RoutedEventArgs e)
        {
            // Берем объект из DataContext кнопки
            if (sender is Button button && button.DataContext is RecipeDto recipe)
            {
                // Открываем детали, передавая весь объект RecipeDto
                var detailsPage = new RecipeDetailsPage(recipe);
                MainWindow.mainWindow.OpenPages(detailsPage);
            }
        }

        private async void RemoveRecipe_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is RecipeDto recipe)
            {
                if (MessageBox.Show($"Удалить рецепт '{recipe.Title}' из избранного?",
                    "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    // Метод RemoveFromFavorites в AppData теперь принимает RecipeDto
                    var success = await AppData.RemoveFromFavorites(recipe);

                    if (success)
                    {
                        // Принудительно удаляем из UI, если ObservableCollection не обновилась
                        if (_favorites.Contains(recipe))
                        {
                            _favorites.Remove(recipe);
                        }
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
            _favorites = AppData.Favorites;
            LoadFavorites();
        }
    }
}