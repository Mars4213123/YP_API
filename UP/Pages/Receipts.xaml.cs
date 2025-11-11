using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using UP.Models;
using UP.Services;

namespace UP.Pages
{
    public partial class Receipts : Page
    {
        public class DailyMenu
        {
            public string Day { get; set; }
            public string Meal { get; set; }
            public string Description { get; set; }
            public int RecipeId { get; set; }
        }

        public class TimerItem
        {
            public string Name { get; set; }
            public string TimeLeft { get; set; }
            public System.Windows.Threading.DispatcherTimer Timer { get; set; }
            public DateTime EndTime { get; set; }
        }

        private ObservableCollection<string> _products;
        private ObservableCollection<DailyMenu> _weeklyMenu;
        private ObservableCollection<TimerItem> _activeTimers;
        private ObservableCollection<string> _shoppingList;

        public Receipts()
        {
            InitializeComponent();
            InitializeData();
        }

        private void InitializeData()
        {
            _products = AppData.Products;
            _weeklyMenu = AppData.WeeklyMenu;
            _shoppingList = AppData.ShoppingList;
            _activeTimers = new ObservableCollection<TimerItem>();

            ProductsListView.ItemsSource = _products;
            WeeklyMenuItemsControl.ItemsSource = _weeklyMenu;
            ActiveTimersItemsControl.ItemsSource = _activeTimers;
            ShoppingListListView.ItemsSource = _shoppingList;

            UpdateNoTimersVisibility();
        }

        private async void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NewProductTextBox.Text))
            {
                var product = NewProductTextBox.Text.Trim();
                _products.Add(product);
                NewProductTextBox.Clear();

                await SaveProductToInventory(product);
            }
        }

        private async Task SaveProductToInventory(string productName)
        {
            try
            {
                var ingredients = await AppData.ApiService.GetIngredientsAsync();
                var ingredient = ingredients.FirstOrDefault(i =>
                    i.Name.Contains(productName));

                if (ingredient != null)
                {
                    var success = await AppData.ApiService.AddToInventoryAsync(
                        ingredient.Id, 1, "шт");

                    if (success)
                    {
                        Console.WriteLine($"Product {productName} added to inventory");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving product to inventory: {ex.Message}");
            }
        }

        private void RemoveProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is string product)
            {
                _products.Remove(product);
            }
        }

        private async void GenerateMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (GenerateMenuButton != null)
                {
                    GenerateMenuButton.IsEnabled = false;
                    GenerateMenuButton.Content = "Генерация...";
                }

                var cuisineTags = GetSelectedCuisineTags();

                if (cuisineTags.Count == 0)
                {
                    cuisineTags = new List<string> { "русская", "европейская", "американская" };
                }

                var request = new GenerateMenuRequest
                {
                    Days = 7,
                    TargetCaloriesPerDay = 2000,
                    CuisineTags = cuisineTags,
                    UseInventory = _products.Count > 0,
                    MealTypes = new List<string> { "breakfast", "lunch", "dinner" }
                };

                var success = await AppData.GenerateNewMenu(request);

                if (success)
                {
                    MessageBox.Show("Меню успешно сгенерировано!", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Не удалось сгенерировать меню. Попробуйте другие настройки.", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка генерации меню: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (GenerateMenuButton != null)
                {
                    GenerateMenuButton.IsEnabled = true;
                    GenerateMenuButton.Content = "🎲 Сгенерировать меню на неделю";
                }
            }
        }

        private List<string> GetSelectedCuisineTags()
        {
            var tags = new List<string>();

            if (GlutenCheckBox?.IsChecked == true)
                tags.Add("безглютеновое");
            if (LactoseCheckBox?.IsChecked == true)
                tags.Add("безлактозное");
            if (NutsCheckBox?.IsChecked == true)
                tags.Add("безореховое");
            if (SeafoodCheckBox?.IsChecked == true)
                tags.Add("безморепродуктов");

            if (!string.IsNullOrWhiteSpace(OtherAllergiesTextBox?.Text))
            {
                var customAllergies = OtherAllergiesTextBox.Text.Split(',')
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrEmpty(t));
                tags.AddRange(customAllergies);
            }

            if (tags.Count == 0)
            {
                tags.AddRange(new[] { "русская", "европейская", "американская" });
            }

            return tags;
        }

        private async void OpenRecipe_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is DailyMenu dailyMenu)
            {
                try
                {
                    if (dailyMenu.RecipeId > 0)
                    {
                        var recipe = await AppData.ApiService.GetRecipeAsync(dailyMenu.RecipeId);

                        if (recipe != null)
                        {
                            var detailsPage = new RecipeDetailsPage(
                                recipe.Title,
                                recipe.Description,
                                recipe.ImageUrl,
                                recipe.Ingredients.ConvertAll(i => $"{i.Name} - {i.Quantity} {i.Unit}"),
                                recipe.Instructions
                            );

                            MainWindow.mainWindow.OpenPages(detailsPage);
                            return;
                        }
                    }

                    var recipeByName = AppData.AllRecipes.FirstOrDefault(r =>
                        r.Title.Equals(dailyMenu.Meal, StringComparison.OrdinalIgnoreCase));

                    if (recipeByName != null)
                    {
                        var detailsPage = new RecipeDetailsPage(
                            recipeByName.Title,
                            recipeByName.Description,
                            recipeByName.ImageUrl,
                            recipeByName.Ingredients.ConvertAll(i => $"{i.Name} - {i.Quantity} {i.Unit}"),
                            recipeByName.Instructions
                        );

                        MainWindow.mainWindow.OpenPages(detailsPage);
                    }
                    else
                    {
                        MessageBox.Show("Рецепт не найден", "Ошибка");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки рецепта: {ex.Message}");
                }
            }
        }

        private void OpenFavorites_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.mainWindow.OpenPages(new FavoritesPage());
        }

        private void StartTimer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is DailyMenu menu)
            {
                var timerWindow = new TimerWindow(menu.Meal);
                if (timerWindow.ShowDialog() == true)
                {
                    var timerItem = new TimerItem
                    {
                        Name = menu.Meal,
                        EndTime = DateTime.Now.AddMinutes(timerWindow.SelectedMinutes),
                        Timer = new System.Windows.Threading.DispatcherTimer()
                    };

                    timerItem.Timer.Interval = TimeSpan.FromSeconds(1);
                    timerItem.Timer.Tick += (s, args) => UpdateTimer(timerItem);
                    timerItem.Timer.Start();

                    _activeTimers.Add(timerItem);
                    UpdateNoTimersVisibility();
                    UpdateTimer(timerItem);
                }
            }
        }

        private void UpdateTimer(TimerItem timerItem)
        {
            var timeLeft = timerItem.EndTime - DateTime.Now;

            if (timeLeft.TotalSeconds <= 0)
            {
                timerItem.Timer.Stop();
                timerItem.TimeLeft = "Время вышло!";
                MessageBox.Show($"Таймер '{timerItem.Name}' завершил работу!", "Таймер",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                timerItem.TimeLeft = $"{timeLeft:mm\\:ss}";
            }

            var index = _activeTimers.IndexOf(timerItem);
            if (index >= 0)
            {
                _activeTimers[index] = timerItem;
            }
        }

        private void StopTimer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is TimerItem timerItem)
            {
                timerItem.Timer.Stop();
                _activeTimers.Remove(timerItem);
                UpdateNoTimersVisibility();
            }
        }

        private void UpdateNoTimersVisibility()
        {
            NoTimersText.Visibility = _activeTimers.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ShoppingItem_Checked(object sender, RoutedEventArgs e)
        {
        }

        private async void ExportShoppingList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Текстовые файлы (*.txt)|*.txt",
                    FileName = "Список_покупок.txt"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var lines = new List<string> { "Список покупок:", "==================" };
                    foreach (var item in _shoppingList)
                    {
                        lines.Add($"- {item}");
                    }
                    System.IO.File.WriteAllLines(saveDialog.FileName, lines);

                    MessageBox.Show("Список покупок экспортирован!", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var timer in _activeTimers)
            {
                timer.Timer.Stop();
            }

            AppData.Logout();
            MainWindow.mainWindow.OpenPages(new LogInPage());
        }

        private void OtherAllergiesTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private async void RefreshData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await AppData.LoadInitialData();
                MessageBox.Show("Данные обновлены", "Успех");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления: {ex.Message}", "Ошибка");
            }
        }
    }

    public class TimerWindow : Window
    {
        public int SelectedMinutes { get; private set; } = 10;

        public TimerWindow(string mealName)
        {
            InitializeComponent();
            Title = $"Таймер для: {mealName}";
        }

        private void InitializeComponent()
        {
            Width = 300;
            Height = 200;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;

            var stackPanel = new StackPanel { Margin = new Thickness(20) };

            stackPanel.Children.Add(new TextBlock
            {
                Text = "Установите время таймера (минуты):",
                Foreground = Brushes.White,
                Margin = new Thickness(0, 0, 0, 10)
            });

            var slider = new Slider
            {
                Minimum = 1,
                Maximum = 120,
                Value = SelectedMinutes,
                Margin = new Thickness(0, 0, 0, 10)
            };
            slider.ValueChanged += (s, e) => SelectedMinutes = (int)e.NewValue;
            stackPanel.Children.Add(slider);

            var valueText = new TextBlock
            {
                Text = $"{SelectedMinutes} мин.",
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };
            stackPanel.Children.Add(valueText);

            slider.ValueChanged += (s, e) =>
            {
                SelectedMinutes = (int)e.NewValue;
                valueText.Text = $"{SelectedMinutes} мин.";
            };

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };

            var okButton = new Button
            {
                Content = "Запустить",
                Background = new SolidColorBrush(Color.FromRgb(76, 175, 80)),
                Foreground = Brushes.White,
                Margin = new Thickness(0, 0, 10, 0)
            };
            okButton.Click += (s, e) => { DialogResult = true; Close(); };
            buttonPanel.Children.Add(okButton);

            var cancelButton = new Button
            {
                Content = "Отмена",
                Background = new SolidColorBrush(Color.FromRgb(244, 67, 54)),
                Foreground = Brushes.White,
            };
            cancelButton.Click += (s, e) => { DialogResult = false; Close(); };
            buttonPanel.Children.Add(cancelButton);

            stackPanel.Children.Add(buttonPanel);

            Content = stackPanel;
            Background = new SolidColorBrush(Color.FromRgb(30, 30, 30));
        }
    }
}