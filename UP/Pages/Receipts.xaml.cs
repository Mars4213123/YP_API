using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        public class AvailableMenu
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public int RecipeCount { get; set; }
            public int TotalDays { get; set; }
        }

        private ObservableCollection<string> _products;
        private ObservableCollection<DailyMenu> _weeklyMenu;
        private ObservableCollection<AvailableMenu> _availableMenus;
        private ObservableCollection<ShoppingListItemDto> _shoppingList;
        private MenuDto _selectedMenu;

        public Receipts()
        {
            InitializeComponent();
            InitializeData();
            LoadUserInfo();
        }

        private void InitializeData()
        {
            _products = AppData.Products;
            _weeklyMenu = AppData.WeeklyMenu;
            _shoppingList = new ObservableCollection<ShoppingListItemDto>();
            _availableMenus = new ObservableCollection<AvailableMenu>();

            ProductsListView.ItemsSource = _products;
            WeeklyMenuItemsControl.ItemsSource = _weeklyMenu;
            AvailableMenusListView.ItemsSource = _availableMenus;
            ShoppingListListView.ItemsSource = _shoppingList;

            // Загружаем доступные меню
            _ = LoadAvailableMenus();
        }

        private void LoadUserInfo()
        {
            if (AppData.CurrentUser != null)
            {
                UserInfoText.Text = $"Пользователь: {AppData.CurrentUser.Username}";
            }
        }

        private async Task LoadAvailableMenus()
        {
            try
            {
                _availableMenus.Clear();

                // Загружаем доступные меню из API
                var menus = await LoadMenusFromApi();

                foreach (var menu in menus)
                {
                    _availableMenus.Add(menu);
                }

                // Если есть текущее меню, показываем его
                if (_weeklyMenu.Any())
                {
                    CurrentMenuTitle.Text = "Текущее меню";
                    CurrentMenuDescription.Text = "Ваше текущее меню на неделю";
                    SelectMenuButton.IsEnabled = false; // Меню уже выбрано
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки меню: {ex.Message}", "Ошибка");
            }
        }

        private async Task<List<AvailableMenu>> LoadMenusFromApi()
        {
            // TODO: Реализовать загрузку доступных меню из API
            // Пример:
            // var response = await AppData.ApiService.GetAvailableMenusAsync();

            // Временные данные для примера
            return new List<AvailableMenu>
            {
                new AvailableMenu
                {
                    Id = 1,
                    Name = "Здоровое питание",
                    Description = "Низкокалорийные рецепты",
                    RecipeCount = 14,
                    TotalDays = 7
                },
                new AvailableMenu
                {
                    Id = 2,
                    Name = "Вегетарианское",
                    Description = "Без мяса и рыбы",
                    RecipeCount = 21,
                    TotalDays = 7
                },
                new AvailableMenu
                {
                    Id = 3,
                    Name = "Быстрое приготовление",
                    Description = "Рецепты до 30 минут",
                    RecipeCount = 14,
                    TotalDays = 7
                }
            };
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
                var success = await AppData.ApiService.AddToInventoryByNameAsync(productName, 1, "шт");
                if (success)
                {
                    MessageBox.Show($"Продукт '{productName}' добавлен в инвентарь", "Успех");
                }
                else
                {
                    MessageBox.Show($"Не удалось добавить продукт '{productName}'", "Ошибка");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
            }
        }

        private void RemoveProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is string product)
            {
                _products.Remove(product);
            }
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
                            var detailsPage = new RecipeDetailsPage(recipe);
                            MainWindow.mainWindow.OpenPages(detailsPage);
                            return;
                        }
                    }

                    var recipeByName = AppData.AllRecipes.FirstOrDefault(r =>
                        r.Title.Equals(dailyMenu.Meal, StringComparison.OrdinalIgnoreCase));

                    if (recipeByName != null)
                    {
                        var detailsPage = new RecipeDetailsPage(recipeByName);
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

        private void MenuCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is AvailableMenu menu)
            {
                // Показываем информацию о выбранном меню
                ShowMenuPreview(menu);
            }
        }

        private void AvailableMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AvailableMenusListView.SelectedItem is AvailableMenu selectedMenu)
            {
                ShowMenuPreview(selectedMenu);
            }
        }

        private void ShowMenuPreview(AvailableMenu menu)
        {
            CurrentMenuTitle.Text = menu.Name;
            CurrentMenuDescription.Text = menu.Description;
            SelectMenuButton.IsEnabled = true;

            // TODO: Загрузить детали меню и показать в центральной панели
        }

        private async void SelectCurrentMenu_Click(object sender, RoutedEventArgs e)
        {
            if (AvailableMenusListView.SelectedItem is AvailableMenu selectedMenu)
            {
                try
                {
                    // TODO: Выбрать меню через API
                    // var success = await AppData.ApiService.SelectMenuAsync(selectedMenu.Id);

                    MessageBox.Show($"Меню '{selectedMenu.Name}' выбрано!", "Успех");
                    await RefreshCurrentMenu();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка выбора меню: {ex.Message}", "Ошибка");
                }
            }
        }

        private async Task RefreshCurrentMenu()
        {
            try
            {
                await AppData.LoadCurrentMenu();
                _weeklyMenu.Clear();
                foreach (var item in AppData.WeeklyMenu)
                {
                    _weeklyMenu.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления меню: {ex.Message}", "Ошибка");
            }
        }

        private async void GenerateShoppingList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // TODO: Генерировать список покупок через API
                // var shoppingList = await AppData.ApiService.GenerateShoppingListAsync(_selectedMenu.Id);

                MessageBox.Show("Список покупок создан", "Успех");
                await RefreshShoppingList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания списка покупок: {ex.Message}", "Ошибка");
            }
        }

        private async Task RefreshShoppingList()
        {
            try
            {
                await AppData.LoadShoppingList();
                _shoppingList.Clear();

                // TODO: Преобразовать данные из AppData.ShoppingList в ShoppingListItemDto
                ShoppingListInfo.Text = "Список покупок загружен";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки списка покупок: {ex.Message}", "Ошибка");
            }
        }

        private void OpenFavorites_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.mainWindow.OpenPages(new FavoritesPage());
        }

        private void ShoppingItem_Checked(object sender, RoutedEventArgs e)
        {
            // TODO: Отметить товар как купленный в API
        }

        private void ShoppingItem_Unchecked(object sender, RoutedEventArgs e)
        {
            // TODO: Снять отметку о покупке в API
        }

        private async void ExportShoppingList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Текстовые файлы (*.txt)|*.txt|Файлы CSV (*.csv)|*.csv",
                    FileName = "Список_покупок.txt"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var lines = new List<string> { "Список покупок", "==================" };
                    foreach (var item in _shoppingList)
                    {
                        lines.Add($"{item.Name} - {item.Quantity} {item.Unit}");
                    }
                    System.IO.File.WriteAllLines(saveDialog.FileName, lines);

                    MessageBox.Show("Список покупок экспортирован!", "Успех");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка");
            }
        }

        private void ClearShoppingList_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Очистить весь список покупок?", "Подтверждение",
                              MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _shoppingList.Clear();
                ShoppingListInfo.Text = "Список покупок очищен";
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            AppData.Logout();
            MainWindow.mainWindow.OpenPages(new LogInPage());
        }

        private void OtherAllergiesTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private async void RefreshMenus_Click(object sender, RoutedEventArgs e)
        {
            await LoadAvailableMenus();
            MessageBox.Show("Список меню обновлен", "Успех");
        }

        private async void RefreshData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await AppData.LoadInitialData();
                await RefreshCurrentMenu();
                await RefreshShoppingList();
                MessageBox.Show("Данные обновлены", "Успех");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления: {ex.Message}", "Ошибка");
            }
        }
    }
}