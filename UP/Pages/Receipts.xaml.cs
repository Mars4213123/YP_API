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
        
        private ObservableCollection<string> _products;
        private ObservableCollection<DailyMenu> _weeklyMenu;
        private ObservableCollection<AvailableMenu> _availableMenus;
        private ObservableCollection<ShoppingListItemDto> _shoppingList;
        private Services.MenuDto _selectedMenu;

        public class AvailableMenu
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public int RecipeCount { get; set; }
            public int TotalDays { get; set; }
        }

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

            Console.WriteLine($"[InitializeData] Инициализируем ItemsSource");
            
            ProductsListView.ItemsSource = _products;
            WeeklyMenuItemsControl.ItemsSource = _weeklyMenu;
            AvailableMenusListView.ItemsSource = _availableMenus;
            ShoppingListListView.ItemsSource = _shoppingList;

            Console.WriteLine($"[InitializeData] ItemsSource привязаны успешно");
            Console.WriteLine($"[InitializeData] _availableMenus тип: {_availableMenus?.GetType().Name}, Count: {_availableMenus?.Count ?? 0}");
            Console.WriteLine($"[InitializeData] AvailableMenusListView.ItemsSource тип: {AvailableMenusListView.ItemsSource?.GetType().Name}");
            
            // Загружаем доступные меню асинхронно
            LoadAvailableMenusOnInit();
        }

        private async void LoadAvailableMenusOnInit()
        {
            Console.WriteLine($"[LoadAvailableMenusOnInit] Запускаем загрузку меню");
            await LoadAvailableMenus();
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
                Console.WriteLine($"[DisplayMenuDetails] Загружаем детали меню {selectedMenu.Id}: {selectedMenu.Name}");
                
                // 1. Получаем полные данные меню (с рецептами)
                var fullMenuDto = await AppData.ApiService.GetMenuDetailsAsync(selectedMenu.Id);

                if (fullMenuDto != null)
                {
                    Console.WriteLine($"[DisplayMenuDetails] Получены детали меню, всего рецептов: {fullMenuDto.Items?.Count ?? 0}");
                    
                    // 2. Обновляем заголовки
                    CurrentMenuTitle.Text = fullMenuDto.Name;
                    CurrentMenuDescription.Text = $"Создано: {fullMenuDto.CreatedAt:dd.MM.yyyy}";

                    // 3. Чистим центральный список
                    _weeklyMenu.Clear();

                    // 4. Заполняем рецептами
                    if (fullMenuDto.Items != null && fullMenuDto.Items.Count > 0)
                    {
                        foreach (var item in fullMenuDto.Items)
                        {
                            Console.WriteLine($"[DisplayMenuDetails] Добавляем блюдо: {item.RecipeTitle} на {item.Date}");
                            
                            _weeklyMenu.Add(new DailyMenu
                            {
                                Day = item.Date.ToString("dddd"), // День недели
                                Meal = item.MealType,             // Тип приема пищи
                                Description = item.RecipeTitle,   // Название блюда
                                RecipeId = item.RecipeId          // ID для клика
                            });
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[DisplayMenuDetails] Меню пусто или не содержит рецептов");
                    }

                    Console.WriteLine($"[DisplayMenuDetails] Добавлено {_weeklyMenu.Count} блюд в UI");

                    // Разблокируем кнопку выбора меню
                    SelectMenuButton.IsEnabled = true;
                    
                    // 5. АВТОМАТИЧЕСКИ ГЕНЕРИРУЕМ СПИСОК ПОКУПОК
                    Console.WriteLine($"[DisplayMenuDetails] Автоматически генерируем список покупок...");
                    await GenerateShoppingListForMenu(selectedMenu.Id);
                }
                else
                {
                    Console.WriteLine($"[DisplayMenuDetails] Детали меню не получены (null)");
                    MessageBox.Show("Не удалось загрузить детали меню", "Ошибка");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DisplayMenuDetails] Exception: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show("Не удалось загрузить меню: " + ex.Message);
            }
        }

        // Новый вспомогательный метод для генерации списка покупок
        private async Task GenerateShoppingListForMenu(int menuId)
        {
            try
            {
                Console.WriteLine($"[GenerateShoppingListForMenu] Генерируем список покупок для меню {menuId}");
                
                // Загружаем рецепты из меню
                var menuDetails = await AppData.ApiService.GetMenuDetailsAsync(menuId);
                
                if (menuDetails?.Items != null && menuDetails.Items.Count > 0)
                {
                    // Собираем все рецепты из меню
                    var recipeIds = menuDetails.Items
                        .Select(item => item.RecipeId)
                        .Distinct()
                        .ToList();

                    Console.WriteLine($"[GenerateShoppingListForMenu] Найдено {recipeIds.Count} уникальных рецептов");
                    
                    _shoppingList.Clear();
                    var ingredientDict = new Dictionary<string, ShoppingListItemDto>();

                    // Для каждого рецепта получаем ингредиенты
                    foreach (var recipeId in recipeIds)
                    {
                        try
                        {
                            var recipe = await AppData.ApiService.GetRecipeAsync(recipeId);
                            
                            if (recipe?.Ingredients != null && recipe.Ingredients.Count > 0)
                            {
                                Console.WriteLine($"[GenerateShoppingListForMenu] Рецепт '{recipe.Title}' содержит {recipe.Ingredients.Count} ингредиентов");
                                
                                foreach (var ingredient in recipe.Ingredients)
                                {
                                    if (ingredient != null && !string.IsNullOrWhiteSpace(ingredient.Name))
                                    {
                                        var key = ingredient.Name.ToLower();
                                        
                                        if (ingredientDict.ContainsKey(key))
                                        {
                                            // Суммируем количество
                                            ingredientDict[key].Quantity += ingredient.Quantity;
                                            Console.WriteLine($"[GenerateShoppingListForMenu] Обновляем {ingredient.Name}: +{ingredient.Quantity} {ingredient.Unit}");
                                        }
                                        else
                                        {
                                            ingredientDict[key] = new ShoppingListItemDto
                                            {
                                                Name = ingredient.Name,
                                                Quantity = ingredient.Quantity,
                                                Unit = ingredient.Unit ?? "шт",
                                                IsPurchased = false
                                            };
                                            Console.WriteLine($"[GenerateShoppingListForMenu] Добавляем {ingredient.Name}: {ingredient.Quantity} {ingredient.Unit}");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine($"[GenerateShoppingListForMenu] Рецепт {recipeId} не содержит ингредиентов");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[GenerateShoppingListForMenu] Ошибка при загрузке рецепта {recipeId}: {ex.Message}");
                        }
                    }

                    // Добавляем в коллекцию
                    foreach (var item in ingredientDict.Values.OrderBy(x => x.Name))
                    {
                        _shoppingList.Add(item);
                    }

                    ShoppingListInfo.Text = $"Загруженo {_shoppingList.Count} товаров";
                    Console.WriteLine($"[GenerateShoppingListForMenu] Список покупок готов: {_shoppingList.Count} товаров");
                }
                else
                {
                    Console.WriteLine($"[GenerateShoppingListForMenu] Меню не содержит рецептов");
                    ShoppingListInfo.Text = "Меню не содержит рецептов";
                    _shoppingList.Clear();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GenerateShoppingListForMenu] Exception: {ex.Message}\n{ex.StackTrace}");
                ShoppingListInfo.Text = "Ошибка при генерации списка покупок";
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
                Console.WriteLine($"[LoadAvailableMenus] Начинаем загрузку меню для пользователя {AppData.CurrentUser?.Id}");
                
                // Очищаем текущий список в UI
                _availableMenus.Clear();

                // Запрашиваем с сервера
                var menusFromApi = await AppData.ApiService.GetUserMenusAsync(AppData.CurrentUser.Id);
                
                Console.WriteLine($"[LoadAvailableMenus] Получено {menusFromApi.Count} меню с сервера");

                foreach (var m in menusFromApi)
                {
                    Console.WriteLine($"[LoadAvailableMenus] Добавляем меню: {m.Name} (ID: {m.Id})");
                    
                    // Преобразуем из сервис-класса в локальный класс
                    var localMenu = new AvailableMenu
                    {
                        Id = m.Id,
                        Name = m.Name,
                        Description = m.Description,
                        RecipeCount = m.RecipeCount,
                        TotalDays = m.TotalDays
                    };
                    
                    // Добавляем в коллекцию, привязанную к ListView
                    _availableMenus.Add(localMenu);
                }
                
                Console.WriteLine($"[LoadAvailableMenus] Всего загружено {_availableMenus.Count} меню в UI");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoadAvailableMenus] Error: {ex.Message}\n{ex.StackTrace}");
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
                    Name = "Быструю приготовление",
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

                // Отправляем продукты в инвентарь (а не в preferences!)
                var setOk = await AppData.ApiService.SetInventoryByNamesAsync(AppData.CurrentUser.Id, productsList);
                if (!setOk)
                {
                    MessageBox.Show("Не удалось сохранить продукты в инвентарь. Проверьте консоль для деталей.", "Ошибка");
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
                
                if (_availableMenus.Count > 0)
                {
                    var lastMenu = _availableMenus.Last();
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
            try
            {
                if (sender is Button button)
                {
                    // Получаем родительский Grid элемент
                    var grid = button.Parent as Grid;
                    if (grid != null)
                    {
                        // Получаем TextBlock с названием продукта
                        if (grid.Children[0] is TextBlock textBlock && textBlock.Text is string productName)
                        {
                            Console.WriteLine($"[RemoveProduct_Click] Удаляем продукт: {productName}");
                            _products.Remove(productName);
                            Console.WriteLine($"[RemoveProduct_Click] Продукт удален. Осталось: {_products.Count}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RemoveProduct_Click] Exception: {ex.Message}");
                MessageBox.Show($"Ошибка удаления продукта: {ex.Message}", "Ошибка");
            }
        }

        private async void OpenRecipe_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button)
                {
                    Console.WriteLine($"[OpenRecipe_Click] Пытаемся открыть рецепт");
                    
                    // Получаем RecipeId из Tag
                    if (button.Tag is int recipeId && recipeId > 0)
                    {
                        Console.WriteLine($"[OpenRecipe_Click] Загружаем рецепт по ID: {recipeId}");
                        
                        var recipe = await AppData.ApiService.GetRecipeAsync(recipeId);

                        if (recipe != null)
                        {
                            Console.WriteLine($"[OpenRecipe_Click] Рецепт найден: {recipe.Title}");
                            var detailsPage = new RecipeDetailsPage(recipe);
                            MainWindow.mainWindow.OpenPages(detailsPage);
                            return;
                        }
                        else
                        {
                            Console.WriteLine($"[OpenRecipe_Click] Рецепт с ID {recipeId} не найден в API");
                            MessageBox.Show("Рецепт не найден", "Ошибка");
                            return;
                        }
                    }
                    
                    // Fallback: пытаемся получить из DataContext
                    var stackPanel = button.Parent as StackPanel;
                    if (stackPanel?.DataContext is DailyMenu dailyMenu && dailyMenu.RecipeId > 0)
                    {
                        Console.WriteLine($"[OpenRecipe_Click] Используем fallback, RecipeId={dailyMenu.RecipeId}");
                        
                        var recipe = await AppData.ApiService.GetRecipeAsync(dailyMenu.RecipeId);
                        if (recipe != null)
                        {
                            var detailsPage = new RecipeDetailsPage(recipe);
                            MainWindow.mainWindow.OpenPages(detailsPage);
                            return;
                        }
                    }

                    MessageBox.Show("Не удалось загрузить рецепт", "Ошибка");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OpenRecipe_Click] Exception: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"Ошибка загрузки рецепта: {ex.Message}", "Ошибка");
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
                if (CurrentMenuTitle.Text == "Текущее меню не выбрано")
                {
                    MessageBox.Show("Сначала выберите меню", "Информация");
                    return;
                }

                Console.WriteLine($"[GenerateShoppingList_Click] Пользователь нажал кнопку списка покупок");
                
                // Получаем ID текущего меню из выбранного элемента
                if (AvailableMenusListView.SelectedItem is AvailableMenu selectedMenu)
                {
                    Console.WriteLine($"[GenerateShoppingList_Click] Переносит список покупок для меню {selectedMenu.Id}");
                    await GenerateShoppingListForMenu(selectedMenu.Id);
                    MessageBox.Show("Список покупок обновлен!", "Успех");
                }
                else
                {
                    MessageBox.Show("Сначала выберите меню из списка", "Информация");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GenerateShoppingList_Click] Exception: {ex.Message}\n{ex.StackTrace}");
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
                // Проверяем что список покупок не пуст
                if (_shoppingList == null || _shoppingList.Count == 0)
                {
                    MessageBox.Show("Список покупок пуст. Сначала выберите меню!", "Информация");
                    return;
                }

                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Текстовые файлы (*.txt)|*.txt|Файлы CSV (*.csv)|*.csv",
                    FileName = "Список_покупок.txt",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                };

                if (saveDialog.ShowDialog() == true)
                {
                    Console.WriteLine($"[ExportShoppingList_Click] Экспортируем {_shoppingList.Count} товаров");
                    
                    var lines = new List<string> 
                    { 
                        "Список покупок",
                        "==================",
                        $"Дата: {DateTime.Now:dd.MM.yyyy HH:mm}",
                        ""
                    };

                    foreach (var item in _shoppingList)
                    {
                        if (item != null)
                        {
                            var status = item.IsPurchased ? "✓" : "○";
                            lines.Add($"{status} {item.Name} - {item.Quantity} {item.Unit}");
                        }
                    }

                    lines.Add("");
                    lines.Add($"Всего товаров: {_shoppingList.Count}");

                    System.IO.File.WriteAllLines(saveDialog.FileName, lines, System.Text.Encoding.UTF8);
                    
                    Console.WriteLine($"[ExportShoppingList_Click] Успешно сохранено в {saveDialog.FileName}");
                    MessageBox.Show($"Список покупок экспортирован в:\n{saveDialog.FileName}", "Успех");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ExportShoppingList_Click] Exception: {ex.Message}");
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

                Console.WriteLine($"[RefreshData_Click] Начинаем процесс обновления с {productsList.Count} продуктами");

                MessageBox.Show($"Сохраняем {productsList.Count} продуктов...", "Информация");

                // Отправляем продукты в инвентарь (а не в preferences!)
                var setOk = await AppData.ApiService.SetInventoryByNamesAsync(AppData.CurrentUser.Id, productsList);
                if (!setOk)
                {
                    MessageBox.Show("Не удалось сохранить продукты в инвентарь. Проверьте консоль для деталей.", "Ошибка");
                    return;
                }

                Console.WriteLine($"[RefreshData_Click] Продукты сохранены, начинаем генерацию меню");
                MessageBox.Show("Продукты сохранены! Генерируем меню...", "Информация");

                // Триггерим генерацию меню
                var genOk = await AppData.ApiService.GenerateMenuAsync(AppData.CurrentUser.Id);
                if (!genOk)
                {
                    Console.WriteLine($"[RefreshData_Click] Меню не сгенерировано, проверяем рецепты");
                    
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

                Console.WriteLine($"[RefreshData_Click] Меню сгенерировано, загружаем список доступных меню");

                // Обновляем список меню и загружаем последнее сгенерированное меню
                await LoadAvailableMenus();
                
                Console.WriteLine($"[RefreshData_Click] Загружено {_availableMenus.Count} меню");
                
                if (_availableMenus.Count > 0)
                {
                    var lastMenu = _availableMenus.Last();
                    Console.WriteLine($"[RefreshData_Click] Выбираем меню: {lastMenu.Name}");
                    
                    AvailableMenusListView.SelectedItem = lastMenu;
                    await DisplayMenuDetails(lastMenu);
                    
                    MessageBox.Show("Холодильник обновлен и меню сгенерировано!", "Успех");
                }
                else
                {
                    MessageBox.Show("Меню сгенерировано, но не удалось его загрузить", "Предупреждение");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RefreshData_Click] Exception: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
            }
        }

        private async void DeleteMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is int menuId)
                {
                    Console.WriteLine($"[DeleteMenu_Click] Удаляем меню {menuId}");
                    
                    var result = MessageBox.Show(
                        "Вы уверены что хотите удалить это меню?",
                        "Подтверждение удаления",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);
                    
                    if (result == MessageBoxResult.Yes)
                    {
                        // Отправляем запрос на удаление меню на сервер
                        var deleteSuccess = await AppData.ApiService.DeleteMenuAsync(menuId);
                        
                        if (deleteSuccess)
                        {
                            Console.WriteLine($"[DeleteMenu_Click] Меню {menuId} удалено успешно");
                            MessageBox.Show("Меню удалено", "Успех");
                            
                            // Перезагружаем список меню
                            await LoadAvailableMenus();
                            
                            // Очищаем центральную панель
                            CurrentMenuTitle.Text = "Текущее меню не выбрано";
                            CurrentMenuDescription.Text = "Выберите меню из списка слева";
                            _weeklyMenu.Clear();
                            SelectMenuButton.IsEnabled = false;
                        }
                        else
                        {
                            Console.WriteLine($"[DeleteMenu_Click] Ошибка при удалении меню {menuId}");
                            MessageBox.Show("Не удалось удалить меню", "Ошибка");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DeleteMenu_Click] Exception: {ex.Message}");
                MessageBox.Show($"Ошибка при удалении меню: {ex.Message}", "Ошибка");
            }
        }
    }
}