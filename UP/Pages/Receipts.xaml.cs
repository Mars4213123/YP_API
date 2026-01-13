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
using static UP.Pages.Receipts;

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
        public ObservableCollection<string> products { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<AvailableMenu> availableMenus { get; set; } = new ObservableCollection<AvailableMenu>();
        public ObservableCollection<DailyMenu> weeklyMenu { get; set; } = new ObservableCollection<DailyMenu>();
        private async void RefreshDataClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AppData.CurrentUser == null) return;

                // 1. Собираем продукты из UI (ObservableCollection -> List)
                var productsList = products.ToList();

                // 2. Отправляем холодильник в базу
                bool fridgeUpdated = await AppData.ApiService.UpdateFridgeAsync(AppData.CurrentUser.Id, productsList);

                if (fridgeUpdated)
                {
                    MessageBox.Show("Холодильник обновлен! Генерируем меню...");

                    // 3. Генерируем меню (Бэкенд сам проверит аллергии и продукты)
                    bool menuGenerated = await AppData.ApiService.GenerateMenuAsync(AppData.CurrentUser.Id);

                    if (menuGenerated)
                    {
                        // 4. Обновляем список меню слева
                        await LoadAvailableMenus();
                        MessageBox.Show("Новое меню создано и добавлено в список!");
                    }
                    else
                    {
                        MessageBox.Show("Не удалось создать меню. Возможно, мало подходящих рецептов.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
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
        private Services.MenuDto _selectedMenu;

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
        private async void AvailableMenuSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Проверяем, что реально что-то выбрано
            if (AvailableMenusListView.SelectedItem is AvailableMenu selectedMenu)
            {
                try
                {
                    // 1. Получаем полные данные меню (с рецептами)
                    var fullMenuDto = await AppData.ApiService.GetMenuDetailsAsync(selectedMenu.Id);

                    if (fullMenuDto != null)
                    {
                        // 2. Обновляем заголовки
                        CurrentMenuTitle.Text = fullMenuDto.Name;
                        CurrentMenuDescription.Text = $"Создано: {fullMenuDto.CreatedAt:dd.MM.yyyy}";

                        // 3. Чистим центральный список
                        weeklyMenu.Clear();

                        // 4. Заполняем рецептами
                        // (Предполагаем, что fullMenuDto.Items содержит список блюд)
                        foreach (var item in fullMenuDto.Items)
                        {
                            weeklyMenu.Add(new DailyMenu
                            {
                                Day = item.Date.ToString("dddd"), // День недели (Пн, Вт...)
                                Meal = item.MealType,             // Тип (Завтрак/Обед)
                                Description = item.RecipeTitle,   // Название блюда
                                RecipeId = item.RecipeId          // ID для клика
                            });
                        }

                        // Разблокируем кнопку "Выбрать это меню" (если нужна логика "Текущее меню")
                        SelectMenuButton.IsEnabled = true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Не удалось загрузить меню: " + ex.Message);
                }
            }
        }
        private async void OpenRecipeClick(object sender, RoutedEventArgs e)
        {
            // Получаем объект данных из кнопки
            if (sender is Button button && button.DataContext is DailyMenu dailyMenu)
            {
                try
                {
                    // Запрашиваем полные данные рецепта по ID
                    var recipe = await AppData.ApiService.GetRecipeAsync(dailyMenu.RecipeId);

                    if (recipe != null)
                    {
                        // Открываем страницу деталей (RecipeDetailsPage)
                        // Убедись, что у RecipeDetailsPage есть конструктор, принимающий RecipeDto или похожий объект
                        var detailsPage = new RecipeDetailsPage(recipe);
                        MainWindow.mainWindow.OpenPages(detailsPage);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка открытия рецепта: " + ex.Message);
                }
            }
        }

        private async Task LoadAvailableMenus()
        {
            try
            {
                // Очищаем текущий список в UI
                availableMenus.Clear();

                // Запрашиваем с сервера
                var menusFromApi = await AppData.ApiService.GetUserMenusAsync(AppData.CurrentUser.Id);

                foreach (var m in menusFromApi)
                {
                    // Добавляем в коллекцию, привязанную к ListView
                    availableMenus.Add(m);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка загрузки меню: " + ex.Message);
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

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            var name = NewProductTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
                return;

            // Инициализация источника, если он ещё null
            if (ProductsListView.ItemsSource is null)
            {
                ProductsListView.ItemsSource = new List<string>();
            }

            var list = ProductsListView.ItemsSource.Cast<string>().ToList();
            list.Add(name);
            ProductsListView.ItemsSource = null;
            ProductsListView.ItemsSource = list;

            NewProductTextBox.Clear();
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

        private async void UpdateFridgeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var userId = AppData.CurrentUser?.Id ?? 0;
                if (userId == 0)
                {
                    MessageBox.Show("Сначала войдите в систему", "Ошибка");
                    return;
                }

                IEnumerable<string> src = ProductsListView.ItemsSource as IEnumerable<string>;
                if (src == null)
                {
                    MessageBox.Show("Введите хотя бы один продукт (по названию)", "Внимание");
                    return;
                }

                var productNames = src
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();

                if (!productNames.Any())
                {
                    MessageBox.Show("Введите хотя бы один продукт (по названию)", "Внимание");
                    return;
                }

                try
                {
                    await AppData.ApiService.SetInventoryByNamesAsync(userId, productNames);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось сохранить продукты: {ex.Message}", "Ошибка");
                    return;
                }

                await AppData.LoadFridgeRecipes(); // внутри вызывает GetRecipesByFridgeAsync(userId)

                ProductsListView.ItemsSource = null;
                ProductsListView.ItemsSource = AppData.FridgeRecipes;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления холодильника: {ex.Message}", "Ошибка");
            }
        }





        private void FridgeRecipesList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ProductsListView.SelectedItem is RecipeDto recipe)
            {
                var page = new RecipeDetailsPage(recipe);
                MainWindow.mainWindow.OpenPages(page);
            }
        }


    }
}