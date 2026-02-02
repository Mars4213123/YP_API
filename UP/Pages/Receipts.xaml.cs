using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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

        private ObservableCollection<string> _products;
        private ObservableCollection<DailyMenu> _weeklyMenu;
        private ObservableCollection<AvailableMenu> _availableMenus;
        private ObservableCollection<ShoppingListItemDto> _shoppingList;

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
            LoadFridgeIngridients();
        }

        private async void LoadFridgeIngridients()
        {
            ProductsListView.ItemsSource = await AppData.ApiService.UpdateFridgeAsync(AppData.CurrentUser.Id);
        }

        private void InitializeData()
        {
            _products = new ObservableCollection<string>();
            _weeklyMenu = AppData.WeeklyMenu;
            _shoppingList = new ObservableCollection<ShoppingListItemDto>();
            _availableMenus = new ObservableCollection<AvailableMenu>();

            if (_products.Count != 0)
                ProductsListView.ItemsSource = _products;

            WeeklyMenuItemsControl.ItemsSource = _weeklyMenu;
            AvailableMenusListView.ItemsSource = _availableMenus;
            ShoppingListListView.ItemsSource = _shoppingList;

            LoadAvailableMenusOnInit();
        }

        private async void LoadAvailableMenusOnInit()
        {
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
            if (AvailableMenusListView.SelectedItem is AvailableMenu selectedMenu)
            {
                await DisplayMenuDetails(selectedMenu);
            }
        }

        private async Task DisplayMenuDetails(AvailableMenu selectedMenu)
        {
            try
            {
                var fullMenuDto = await AppData.ApiService.GetMenuDetailsAsync(selectedMenu.Id);

                if (fullMenuDto == null)
                {
                    MessageBox.Show("Не удалось загрузить детали меню", "Ошибка");
                    return;
                }

                CurrentMenuTitle.Text = fullMenuDto.Name;
                CurrentMenuDescription.Text = $"Создано: {fullMenuDto.CreatedAt:dd.MM.yyyy}";

                _weeklyMenu.Clear();

                if (fullMenuDto.Items != null && fullMenuDto.Items.Count > 0)
                {
                    foreach (var item in fullMenuDto.Items)
                    {
                        _weeklyMenu.Add(new DailyMenu
                        {
                            Day = item.Date.ToString("dddd"),
                            Meal = item.MealType,
                            Description = item.RecipeTitle,
                            RecipeId = item.RecipeId
                        });
                    }
                }

                await GenerateShoppingListForMenu(selectedMenu.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось загрузить меню: " + ex.Message, "Ошибка");
            }
        }

        private async Task GenerateShoppingListForMenu(int menuId)
        {
            try
            {
                var menuDetails = await AppData.ApiService.GetMenuDetailsAsync(menuId);

                if (menuDetails == null || menuDetails.Items == null || menuDetails.Items.Count == 0)
                {
                    ShoppingListInfo.Text = "Меню не содержит рецептов";
                    _shoppingList.Clear();
                    return;
                }

                var recipeIds = menuDetails.Items.Select(item => item.RecipeId).Distinct().ToList();
                _shoppingList.Clear();
                var ingredientDict = new Dictionary<string, ShoppingListItemDto>();

                foreach (var recipeId in recipeIds)
                {
                    try
                    {
                        var recipe = await AppData.ApiService.GetRecipeAsync(recipeId);
                        if (recipe == null || recipe.Ingredients == null) continue;

                        foreach (var ingredient in recipe.Ingredients)
                        {
                            if (ingredient == null || string.IsNullOrWhiteSpace(ingredient.Name)) continue;

                            var key = ingredient.Name.ToLower();
                            if (ingredientDict.ContainsKey(key))
                            {
                                ingredientDict[key].Quantity += ingredient.Quantity;
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
                            }
                        }
                    }
                    catch
                    {
                        // Игнорируем ошибки отдельных рецептов
                    }
                }

                foreach (var item in ingredientDict.Values.OrderBy(x => x.Name))
                {
                    _shoppingList.Add(item);
                }

                ShoppingListInfo.Text = $"Загружено {_shoppingList.Count} товаров";
            }
            catch (Exception ex)
            {
                ShoppingListInfo.Text = "Ошибка при генерации списка покупок";
                MessageBox.Show("Не удалось сгенерировать список покупок: " + ex.Message, "Ошибка");
            }
        }

        private async void OpenRecipe_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int? recipeId = null;

                if (sender is Button button)
                {
                    // Попытка из Tag
                    if (button.Tag is int id && id > 0)
                    {
                        recipeId = id;
                    }
                    // Попытка из DataContext
                    else if (button.DataContext is DailyMenu dailyMenu && dailyMenu.RecipeId > 0)
                    {
                        recipeId = dailyMenu.RecipeId;
                    }
                }

                if (recipeId.HasValue)
                {
                    var recipe = await AppData.ApiService.GetRecipeAsync(recipeId.Value);
                    if (recipe != null)
                    {
                        var detailsPage = new RecipeDetailsPage(recipe);
                        MainWindow.mainWindow.OpenPages(detailsPage);
                        return;
                    }
                }

                MessageBox.Show("Не удалось загрузить рецепт", "Ошибка");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки рецепта: {ex.Message}", "Ошибка");
            }
        }

        private async Task LoadAvailableMenus()
        {
            try
            {
                _availableMenus.Clear();

                var menusFromApi = await AppData.ApiService.GetUserMenusAsync(AppData.CurrentUser.Id);

                foreach (var m in menusFromApi)
                {
                    _availableMenus.Add(new AvailableMenu
                    {
                        Id = m.Id,
                        Name = m.Name,
                        Description = m.Description,
                        RecipeCount = m.RecipeCount,
                        TotalDays = m.TotalDays
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось загрузить список меню: " + ex.Message, "Ошибка");
            }
        }

        private async void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            var name = NewProductTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(name)) return;

            var product = await AppData.ApiService.FindIngredientByNameAsync(name);

            if (!_products.Contains(product.Name))
            {
                _products.Add(product.Name);
                 await AppData.ApiService.AddFridgeItem(AppData.CurrentUser.Id, name);
            }
            else {
                MessageBox.Show("Не удалось найти продукт!");
            }

            NewProductTextBox.Clear();
        }

        private async void UpdateFridgeButton_Click(object sender, RoutedEventArgs e)
        {
            await RefreshFridgeAndGenerateMenu();
        }

        private async void RefreshData_Click(object sender, RoutedEventArgs e)
        {
            await RefreshFridgeAndGenerateMenu();
        }

        private async Task RefreshFridgeAndGenerateMenu()
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

                var setOk = await AppData.ApiService.SetInventoryByNamesAsync(AppData.CurrentUser.Id, productsList);
                if (!setOk)
                {
                    MessageBox.Show("Не удалось сохранить продукты в инвентарь", "Ошибка");
                    return;
                }

                var genOk = await AppData.ApiService.GenerateMenuAsync(AppData.CurrentUser.Id);
                if (!genOk)
                {
                    var fridgeRecipes = await AppData.ApiService.GetRecipesByFridgeAsync(AppData.CurrentUser.Id);
                    if (fridgeRecipes == null || fridgeRecipes.Count == 0)
                    {
                        MessageBox.Show("Найдено 0 рецептов из ваших продуктов", "Информация");
                        return;
                    }
                }
                
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
            }
        }

        private void RemoveProduct_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Parent is Grid grid && grid.Children.Count > 0)
                {
                    var firstChild = grid.Children[0];
                    if (firstChild is TextBlock textBlock)
                    {
                        var productName = textBlock.Text;
                        _products.Remove(productName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления продукта: {ex.Message}", "Ошибка");
            }
        }


        private async void GenerateShoppingList_Click(object sender, RoutedEventArgs e)
        {
            if (!(AvailableMenusListView.SelectedItem is AvailableMenu selectedMenu))
            {
                MessageBox.Show("Сначала выберите меню из списка", "Информация");
                return;
            }

            await GenerateShoppingListForMenu(selectedMenu.Id);
            MessageBox.Show("Список покупок обновлен!", "Успех");
        }

        private async void ExportShoppingList_Click(object sender, RoutedEventArgs e)
        {
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
                try
                {
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
                    MessageBox.Show($"Список покупок экспортирован в:\n{saveDialog.FileName}", "Успех");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка");
                }
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

        private async void RefreshMenus_Click(object sender, RoutedEventArgs e)
        {
            await LoadAvailableMenus();
            MessageBox.Show("Список меню обновлен", "Успех");
        }

        private async void DeleteMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int menuId)
            {
                var result = MessageBox.Show(
                    "Вы уверены что хотите удалить это меню?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var deleteSuccess = await AppData.ApiService.DeleteMenuAsync(menuId);

                        if (deleteSuccess)
                        {
                            await LoadAvailableMenus();

                            CurrentMenuTitle.Text = "Текущее меню не выбрано";
                            CurrentMenuDescription.Text = "Выберите меню из списка слева";
                            _weeklyMenu.Clear();

                            MessageBox.Show("Меню удалено", "Успех");
                        }
                        else
                        {
                            MessageBox.Show("Не удалось удалить меню", "Ошибка");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении меню: {ex.Message}", "Ошибка");
                    }
                }
            }
        }

        private void OpenFavorites_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.mainWindow.OpenPages(new FavoritesPage());
        }

        private async void SelectionChangedMenu(object sender, SelectionChangedEventArgs e)
        {
            if (AvailableMenusListView.SelectedItem is AvailableMenu selectedMenu)
            {
                try
                {
                    await DisplayMenuDetails(selectedMenu);
                    MessageBox.Show($"Меню '{selectedMenu.Name}' выбрано!", "Успех");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка выбора меню: {ex.Message}", "Ошибка");
                }
            }
        }
    }
}