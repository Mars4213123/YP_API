using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Text.Json;
using System.Linq;

namespace UP.Pages
{
    public partial class FavoritesPage : Page
    {
        public class FavoriteRecipe
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Category { get; set; }
            public string Description { get; set; }
            public DateTime AddedDate { get; set; }
            public int UsageCount { get; set; }
            public string CookingTime { get; set; }
            public string Cuisine { get; set; }
        }

        private ObservableCollection<FavoriteRecipe> favoriteRecipes = new ObservableCollection<FavoriteRecipe>();
        private string favoritesFilePath = "favorites.json";

        public FavoritesPage()
        {
            InitializeComponent();
            FavoritesItemsControl.ItemsSource = favoriteRecipes;
            LoadFavorites();
        }

        private void LoadFavorites()
        {
            try
            {
                if (File.Exists(favoritesFilePath))
                {
                    string json = File.ReadAllText(favoritesFilePath);
                    var recipes = JsonSerializer.Deserialize<ObservableCollection<FavoriteRecipe>>(json);

                    favoriteRecipes.Clear();
                    foreach (var recipe in recipes)
                    {
                        favoriteRecipes.Add(recipe);
                    }
                }
                else
                {
                    // Загрузка тестовых данных
                    LoadSampleFavorites();
                }

                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки избранного: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                LoadSampleFavorites();
            }
        }

        private void LoadSampleFavorites()
        {
            favoriteRecipes.Clear();

            favoriteRecipes.Add(new FavoriteRecipe
            {
                Id = "1",
                Name = "Овсяная каша с фруктами",
                Category = "Завтрак",
                Description = "Питательный завтрак с ягодами и медом",
                AddedDate = DateTime.Now.AddDays(-2),
                UsageCount = 5,
                CookingTime = "15 мин",
                Cuisine = "Европейская"
            });

            favoriteRecipes.Add(new FavoriteRecipe
            {
                Id = "2",
                Name = "Куриный суп с лапшой",
                Category = "Обед",
                Description = "Ароматный суп с курицей и овощами",
                AddedDate = DateTime.Now.AddDays(-5),
                UsageCount = 3,
                CookingTime = "40 мин",
                Cuisine = "Русская"
            });

            favoriteRecipes.Add(new FavoriteRecipe
            {
                Id = "3",
                Name = "Паста Карбонара",
                Category = "Ужин",
                Description = "Классическая итальянская паста",
                AddedDate = DateTime.Now.AddDays(-10),
                UsageCount = 8,
                CookingTime = "25 мин",
                Cuisine = "Итальянская"
            });
        }

        private void ApplyFilters()
        {
            var filteredRecipes = favoriteRecipes.AsEnumerable();

            if (RecentRecipesRadio.IsChecked == true)
            {
                filteredRecipes = filteredRecipes.Where(r => r.AddedDate >= DateTime.Now.AddDays(-7));
            }
            else if (MostUsedRadio.IsChecked == true)
            {
                filteredRecipes = filteredRecipes.OrderByDescending(r => r.UsageCount);
            }
            else
            {
                filteredRecipes = filteredRecipes.OrderByDescending(r => r.AddedDate);
            }

            FavoritesItemsControl.ItemsSource = new ObservableCollection<FavoriteRecipe>(filteredRecipes);
        }

        private void ViewRecipe_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is FavoriteRecipe recipe)
            {
                // Увеличиваем счетчик использования
                recipe.UsageCount++;
                SaveFavorites();

                // Открываем страницу с рецептом
                var recipePage = new Receipts(recipe.Name);
                NavigationService.Navigate(recipePage);
            }
        }

        private void RemoveFromFavorites_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is FavoriteRecipe recipe)
            {
                var result = MessageBox.Show($"Удалить рецепт '{recipe.Name}' из избранного?",
                                           "Удаление из избранного",
                                           MessageBoxButton.YesNo,
                                           MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    favoriteRecipes.Remove(recipe);
                    SaveFavorites();
                    ApplyFilters();

                    MessageBox.Show("Рецепт удален из избранного", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void SaveFavorites()
        {
            try
            {
                string json = JsonSerializer.Serialize(favoriteRecipes, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(favoritesFilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения избранного: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обработчики радио-кнопок
        private void AllRecipesRadio_Checked(object sender, RoutedEventArgs e) => ApplyFilters();
        private void RecentRecipesRadio_Checked(object sender, RoutedEventArgs e) => ApplyFilters();
        private void MostUsedRadio_Checked(object sender, RoutedEventArgs e) => ApplyFilters();

        public void AddToFavorites(FavoriteRecipe recipe)
        {
            // Проверяем, нет ли уже такого рецепта в избранном
            var existingRecipe = favoriteRecipes.FirstOrDefault(r => r.Id == recipe.Id);
            if (existingRecipe == null)
            {
                recipe.AddedDate = DateTime.Now;
                favoriteRecipes.Add(recipe);
                SaveFavorites();
                ApplyFilters();

                MessageBox.Show("Рецепт добавлен в избранное!", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Этот рецепт уже в избранном", "Информация",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}