using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using UP.Models; // Убедись, что тут твои DTO (RecipeDto и т.д.)

namespace UP.Pages
{
    public partial class RecipeDetailsPage : Page
    {
        // Вспомогательные классы для отображения в списках XAML
        public class IngredientViewItem
        {
            public string Name { get; set; }
            public string Amount { get; set; }
        }

        public class StepViewItem
        {
            public int Number { get; set; }
            public string Text { get; set; }
        }

        private RecipeDto _currentRecipe;
        private int _recipeId;

        // Конструктор, если переходим по ID (нужна загрузка)
        public RecipeDetailsPage(int recipeId)
        {
            InitializeComponent();
            _recipeId = recipeId;
            Loaded += Page_Loaded;
        }

        // Конструктор, если передаем готовый объект (из списка)
        public RecipeDetailsPage(RecipeDto recipe)
        {
            InitializeComponent();
            _currentRecipe = recipe;
            _recipeId = recipe.Id;
            DisplayRecipe(_currentRecipe);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Если рецепт не передан, но есть ID — загружаем с сервера
            if (_currentRecipe == null && _recipeId > 0)
            {
                try
                {
                    // Метод GetRecipeAsync должен быть реализован в твоем ApiService
                    var recipe = await AppData.ApiService.GetRecipeAsync(_recipeId);

                    if (recipe != null)
                    {
                        _currentRecipe = recipe;
                        DisplayRecipe(_currentRecipe);
                    }
                    else
                    {
                        MessageBox.Show("Рецепт не найден.", "Ошибка");
                        if (NavigationService.CanGoBack) NavigationService.GoBack();
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

            // 1. Заполняем текстовые поля
            RecipeTitleText.Text = recipe.Title;
            RecipeDescriptionText.Text = recipe.Description;

            // 2. Заполняем статистику (Чипсы)
            // Время (Подготовка + Готовка)
            int totalMinutes = recipe.PrepTime + recipe.CookTime;
            PrepTimeText.Text = $"{totalMinutes} мин";

            // Калории
            CaloriesText.Text = $"{Math.Round(recipe.Calories)} ккал";

            // Количество ингредиентов
            int ingCount = recipe.Ingredients?.Count ?? 0;
            IngredientsCountText.Text = $"{ingCount} инг.";

            // 3. Загрузка изображения
            LoadImage(recipe.ImageUrl);

            if (recipe.Ingredients != null)
            {
                var viewItems = recipe.Ingredients.Select(i => new IngredientViewItem
                {
                    Name = i.Name,
                    Amount = $"{i.Quantity:0.##} {i.Unit}"
                }).ToList();

                if (viewItems.Count == 0)
                {
                    viewItems.Add(new IngredientViewItem { Name = "Ингредиенты не указаны", Amount = "" });
                }

                IngredientsList.ItemsSource = viewItems;
            }

            if (recipe.Instructions != null && recipe.Instructions.Any())
            {
                int stepNum = 1;
                var steps = recipe.Instructions.Select(instr => new StepViewItem
                {
                    Number = stepNum++,
                    Text = instr.Trim()
                }).ToList();

                StepsList.ItemsSource = steps;
            }
            else
            {
                StepsList.ItemsSource = new List<StepViewItem>
                {
                    new StepViewItem { Number = 1, Text = "Инструкции по приготовлению отсутствуют." }
                };
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
                RecipeImage.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/DefaultRecipe.png"));
            }
            catch
            {
                RecipeImage.Visibility = Visibility.Collapsed;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }

        private async void AddToShoppingList_Click(object sender, RoutedEventArgs e)
        {
            if (_currentRecipe == null) return;

            try
            {
                //bool success = await AppData.ApiService.ToggleFavoriteAsync(AppData.CurrentUserId, _currentRecipe.Id);

                //if (success)
                //    MessageBox.Show("Ингредиенты успешно добавлены в список покупок!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                //else
                //    MessageBox.Show("Не удалось добавить ингредиенты.", "Ошибка");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка соединения: {ex.Message}", "Ошибка");
            }
        }

        private async void AddToFavorites_Click(object sender, RoutedEventArgs e)
        {
            if (_currentRecipe == null) return;

            try
            {
                bool success = await AppData.ApiService.ToggleFavoriteAsync(_currentRecipe.Id);

                if (success)
                    MessageBox.Show("Рецепт добавлен в избранное!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show("Этот рецепт уже в вашем избранном.", "Инфо", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
            }
        }
    }
}