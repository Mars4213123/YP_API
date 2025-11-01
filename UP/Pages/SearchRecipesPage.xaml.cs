using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace UP.Pages
{
    public partial class RecipeDetailPage : Page
    {
        private string currentRecipeName;

        public RecipeDetailPage(string recipeName)
        {
            InitializeComponent();
            currentRecipeName = recipeName;
            LoadRecipeData(recipeName);
        }

        public RecipeDetailPage() : this("Овсяная каша с фруктами")
        {
            // Конструктор по умолчанию для тестирования
        }

        private void LoadRecipeData(string recipeName)
        {
            try
            {
                RecipeTitle.Text = recipeName;

                // Загрузка данных рецепта (заглушка)
                switch (recipeName)
                {
                    case "Овсяная каша с фруктами":
                        CookingTimeText.Text = "15 мин";
                        CaloriesText.Text = "350 ккал";
                        CuisineText.Text = "Европейская";

                        var ingredients = new ObservableCollection<string>
                        {
                            "• Овсяные хлопья - 100г",
                            "• Молоко - 200мл",
                            "• Банан - 1 шт",
                            "• Мед - 1 ст.л.",
                            "• Ягоды - 50г"
                        };
                        IngredientsList.ItemsSource = ingredients;

                        var steps = new ObservableCollection<string>
                        {
                            "1. Доведите молоко до кипения",
                            "2. Добавьте овсяные хлопья и варите 10 минут",
                            "3. Нарежьте банан и добавьте в кашу",
                            "4. Подавайте с ягодами и медом"
                        };
                        StepsList.ItemsSource = steps;
                        break;

                    case "Куриный суп с лапшой":
                        CookingTimeText.Text = "40 мин";
                        CaloriesText.Text = "250 ккал";
                        CuisineText.Text = "Русская";

                        ingredients = new ObservableCollection<string>
                        {
                            "• Куриное филе - 300г",
                            "• Лапша - 100г",
                            "• Морковь - 1 шт",
                            "• Лук - 1 шт",
                            "• Картофель - 2 шт"
                        };
                        IngredientsList.ItemsSource = ingredients;

                        steps = new ObservableCollection<string>
                        {
                            "1. Сварите куриный бульон",
                            "2. Добавьте нарезанные овощи",
                            "3. Варите 20 минут",
                            "4. Добавьте лапшу и варите еще 10 минут"
                        };
                        StepsList.ItemsSource = steps;
                        break;

                    default:
                        // Загрузка данных по умолчанию
                        CookingTimeText.Text = "30 мин";
                        CaloriesText.Text = "400 ккал";
                        CuisineText.Text = "Международная";

                        ingredients = new ObservableCollection<string>
                        {
                            "• Ингредиенты не найдены"
                        };
                        IngredientsList.ItemsSource = ingredients;

                        steps = new ObservableCollection<string>
                        {
                            "Рецепт находится в разработке"
                        };
                        StepsList.ItemsSource = steps;
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки рецепта: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StartTimer_Click(object sender, RoutedEventArgs e)
        {
            // Переход на страницу таймеров
            NavigationService?.Navigate(new TimersPage());
        }

        private void AddToFavorites_Click(object sender, RoutedEventArgs e)
        {
            var favoriteRecipe = new FavoritesPage.FavoriteRecipe
            {
                Id = Guid.NewGuid().ToString(),
                Name = currentRecipeName,
                Category = "Основное блюдо",
                Description = "Вкусный и полезный рецепт",
                AddedDate = DateTime.Now,
                UsageCount = 1,
                CookingTime = CookingTimeText.Text,
                Cuisine = CuisineText.Text
            };

            // Добавляем в избранное
            var favoritesPage = new FavoritesPage();
            favoritesPage.AddToFavorites(favoriteRecipe);

            MessageBox.Show("Рецепт добавлен в избранное!", "Успех",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}