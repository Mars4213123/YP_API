using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using UP.Models;
using UP.Pages;

namespace UP.Pages
{
    public partial class FavoritesPage : Page
    {
        private ObservableCollection<RecipeDetailsPage.RecipeData> _favorites;

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
                Console.WriteLine($"[LoadFavorites] Загружаем {_favorites.Count} рецептов");
                FavoritesList.ItemsSource = _favorites;
                
                if (_favorites.Count == 0)
                {
                    Console.WriteLine("[LoadFavorites] Список избранного пуст");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoadFavorites] Error: {ex.Message}");
                MessageBox.Show($"Ошибка загрузки избранного: {ex.Message}", "Ошибка");
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.mainWindow.OpenPages(new Receipts());
        }

        private async void OpenRecipe_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.DataContext is RecipeDetailsPage.RecipeData recipe)
                {
                    Console.WriteLine($"[OpenRecipe_Click] Открываем рецепт: {recipe.Title}");
                    
                    // Пытаемся найти в AllRecipes для получения полных данных
                    var apiRecipe = AppData.AllRecipes.FirstOrDefault(r => r.Title == recipe.Title);

                    if (apiRecipe != null)
                    {
                        Console.WriteLine($"[OpenRecipe_Click] Найден в AllRecipes, открываем полную версию");
                        var detailsPage = new RecipeDetailsPage(apiRecipe);
                        MainWindow.mainWindow.OpenPages(detailsPage);
                    }
                    else
                    {
                        Console.WriteLine($"[OpenRecipe_Click] Открываем из локального хранилища");
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
            catch (Exception ex)
            {
                Console.WriteLine($"[OpenRecipe_Click] Exception: {ex.Message}");
                MessageBox.Show($"Ошибка открытия рецепта: {ex.Message}", "Ошибка");
            }
        }

        private async void RemoveRecipe_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.DataContext is RecipeDetailsPage.RecipeData recipe)
                {
                    Console.WriteLine($"[RemoveRecipe_Click] Удаляем рецепт: {recipe.Title}");
                    
                    if (MessageBox.Show(
                        $"Удалить рецепт '{recipe.Title}' из избранного?",
                        "Подтверждение",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        var success = await AppData.RemoveFromFavorites(recipe);
                        
                        if (success)
                        {
                            Console.WriteLine($"[RemoveRecipe_Click] Успешно удалено со слоя, удаляем из локального списка");
                            _favorites.Remove(recipe);
                            MessageBox.Show("Рецепт удален из избранного", "Успех");
                        }
                        else
                        {
                            Console.WriteLine($"[RemoveRecipe_Click] Ошибка удаления");
                            MessageBox.Show("Не удалось удалить рецепт", "Ошибка");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RemoveRecipe_Click] Exception: {ex.Message}");
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
            }
        }

        private async void RefreshFavorites_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine("[RefreshFavorites_Click] Обновляем список избранного");
                await AppData.LoadFavorites();
                LoadFavorites();
                MessageBox.Show("Список обновлен", "Успех");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RefreshFavorites_Click] Exception: {ex.Message}");
                MessageBox.Show($"Ошибка обновления: {ex.Message}", "Ошибка");
            }
        }
    }
}