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
                        
                        // 5. Автоматически выбираем и загружаем последнее сгенерированное меню
                        if (availableMenus.Count > 0)
                        {
                            var lastMenu = availableMenus.Last();
                            AvailableMenusListView.SelectedItem = lastMenu;
                            await DisplayMenuDetails(lastMenu);
                        }
                        
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
                await DisplayMenuDetails(selectedMenu);
            }
        }

        private async Task DisplayMenuDetails(AvailableMenu selectedMenu)
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
                    if (fullMenuDto.Items != null)
                    {
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

            // Добавляем продукт в общую коллекцию AppData.Products
            if (!_products.Contains(name))
            {
                _products.Add(name);
            }

            NewProductTextBox.Clear();
        }

        // Обработчик кнопки "Обновить" в секции холодильника
        private async void UpdateFridgeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AppData.CurrentUser == null)
                {
                    MessageBox.Show("Пользователь не авторизован", "Ошибка");
                    return;
                }

                var productsList = _products.ToList();
                if (!productsList.Any())
                {
                    MessageBox.Show("В холодильнике нет продуктов", "Информация");
                    return;
                }

                MessageBox.Show($"Сохраняем {productsList.Count} продуктов...", "Информация");

                // Отправляем продукты в бэкенд (создаст ингредиенты, если нужно)
                var setOk = await AppData.ApiService.SetFridgeByNamesAsync(AppData.CurrentUser.Id, productsList);
                if (!setOk)
                {
                    MessageBox.Show("Не удалось сохранить продукты в холодильник. Проверьте консоль для деталей.", "Ошибка");
                    return;
                }

                MessageBox.Show("Продукты сохранены! Генерируем меню...", "Информация");

                // Триггерим генерацию меню
                var genOk = await AppData.ApiService.GenerateMenuAsync(AppData.CurrentUser.Id);
                if (!genOk)
                {
                    // Попробуем загрузить рецепты по холодильнику, чтобы понять причину
                    var fridgeRecipes = await AppData.ApiService.GetRecipesByFridgeAsync(AppData.CurrentUser.Id);
                    if (fridgeRecipes == null || fridgeRecipes.Count == 0)
                    {
                        MessageBox.Show("Найдено 0 рецептов из ваших продуктов", "Информация");
                        return;
                    }
                    else
                    {
                        MessageBox.Show("Меню не сгенерировано, но найдены рецепты в холодильнике", "Информация");
                    }
                }

                // Обновляем список меню и загружаем последнее сгенерированное меню
                await LoadAvailableMenus();
                
                if (availableMenus.Count > 0)
                {
                    var lastMenu = availableMenus.Last();
                    AvailableMenusListView.SelectedItem = lastMenu;
                    await DisplayMenuDetails(lastMenu);
                }
                
                MessageBox.Show("Холодильник обновлен и меню сгенерировано!", "Успех");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
                Console.WriteLine($"UpdateFridgeButton_Click Exception: {ex}");
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
                    // Загружаем и отображаем выбранное меню
                    await DisplayMenuDetails(selectedMenu);
                    MessageBox.Show($"Меню '{selectedMenu.Name}' выбрано!", "Успех");
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
            await LoadMenusFromApi();
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

        private async void UpdateFridgeButton_Click_Alt(object sender, RoutedEventArgs e)
        {
            try
            {
                var userId = AppData.CurrentUser?.Id ?? 0;
                if (userId == 0)
                {
                    MessageBox.Show("Сначала войдите в систему", "Ошибка");
                    return;
                }

                // Получаем список строк из UI
                IEnumerable<string> src = null;

                // ВАЖНО: Проверяем, что сейчас в ItemsSource - строки или рецепты
                if (ProductsListView.ItemsSource is ObservableCollection<string> collection)
                {
                    src = collection;
                }
                else if (ProductsListView.ItemsSource is List<string> list)
                {
                    src = list;
                }
                else
                {
                    // Если там уже рецепты или null, берем из AppData.Products
                    src = AppData.Products;
                }

                var productNames = src?
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();

                if (productNames == null || !productNames.Any())
                {
                    MessageBox.Show("Список продуктов пуст. Добавьте продукты.", "Внимание");
                    return;
                }

                // Отправляем на сервер (используем правильный метод!)
                try
                {
                    await AppData.ApiService.AddProductsToFridgeAsync(userId, productNames);

                    MessageBox.Show("Продукты сохранены!", "Успех");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось сохранить продукты: {ex.Message}", "Ошибка");
                    return;
                }

                await AppData.LoadFridgeRecipes();

                weeklyMenu.Clear();
                foreach (var r in AppData.FridgeRecipes)
                {
                    weeklyMenu.Add(new DailyMenu
                    {
                        Day = "Холодильник",
                        Meal = "Рецепт",
                        Description = r.Title,
                        RecipeId = r.Id
                    });
                }

                MessageBox.Show($"Найдено {AppData.FridgeRecipes.Count} рецептов из ваших продуктов.", "Результат");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
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