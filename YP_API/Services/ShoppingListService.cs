using Microsoft.EntityFrameworkCore;
using YP_API.Interfaces;
using YP_API.Models;
using YP_API.Repositories;

namespace YP_API.Services
{
    public class ShoppingListService : IShoppingListService
    {
        private readonly IShoppingListRepository _shoppingListRepository;
        private readonly IMenuRepository _menuRepository;
        private readonly IIngredientRepository _ingredientRepository;
        private readonly IRepository<ShoppingList> _shoppingListRepo;
        private readonly IRepository<ShoppingListItem> _shoppingListItemRepo;

        public ShoppingListService(
            IShoppingListRepository shoppingListRepository,
            IMenuRepository menuRepository,
            IIngredientRepository ingredientRepository,
            IRepository<ShoppingList> shoppingListRepo,
            IRepository<ShoppingListItem> shoppingListItemRepo)
        {
            _shoppingListRepository = shoppingListRepository;
            _menuRepository = menuRepository;
            _ingredientRepository = ingredientRepository;
            _shoppingListRepo = shoppingListRepo;
            _shoppingListItemRepo = shoppingListItemRepo;
        }

        public async Task<ShoppingList> GenerateShoppingListFromMenuAsync(int menuId, int userId)
        {
            try
            {
                Console.WriteLine($"=== START GenerateShoppingListFromMenuAsync ===");
                Console.WriteLine($"MenuId: {menuId}, UserId: {userId}");

                var menu = await _menuRepository.GetByIdAsync(menuId);
                if (menu == null)
                {
                    throw new Exception($"Меню с ID {menuId} не найдено");
                }

                // Проверяем, что меню принадлежит пользователю
                if (menu.UserId != userId)
                {
                    throw new Exception("Меню не принадлежит пользователю");
                }

                // Получаем меню с деталями (рецептами и ингредиентами)
                var menuWithDetails = await _menuRepository.GetMenuWithDetailsAsync(menuId);
                if (menuWithDetails == null)
                {
                    throw new Exception("Не удалось загрузить детали меню");
                }

                Console.WriteLine($"Menu has {menuWithDetails.MenuMeals?.Count} meals");

                // Создаем список покупок
                var shoppingList = new ShoppingList
                {
                    MenuId = menuId,
                    UserId = userId,
                    Name = $"Список покупок для {menu.Name}",
                    CreatedAt = DateTime.UtcNow,
                    IsCompleted = false,
                    Items = new List<ShoppingListItem>() // Инициализируем коллекцию
                };

                // Собираем все ингредиенты из всех рецептов меню
                var ingredientQuantities = new Dictionary<int, (decimal Quantity, string Unit, string Category)>();

                foreach (var menuMeal in menuWithDetails.MenuMeals ?? new List<MenuMeal>())
                {
                    var recipe = menuMeal.Recipe;
                    if (recipe?.RecipeIngredients == null) continue;

                    foreach (var recipeIngredient in recipe.RecipeIngredients)
                    {
                        var ingredient = recipeIngredient.Ingredient;
                        if (ingredient == null) continue;

                        if (ingredientQuantities.ContainsKey(ingredient.Id))
                        {
                            // Суммируем количество если ингредиент уже есть
                            var existing = ingredientQuantities[ingredient.Id];
                            ingredientQuantities[ingredient.Id] = (
                                existing.Quantity + recipeIngredient.Quantity,
                                existing.Unit,
                                existing.Category
                            );
                        }
                        else
                        {
                            // Добавляем новый ингредиент
                            ingredientQuantities[ingredient.Id] = (
                                recipeIngredient.Quantity,
                                recipeIngredient.Unit,
                                ingredient.Category
                            );
                        }
                    }
                }

                // Создаем элементы списка покупок
                foreach (var (ingredientId, (quantity, unit, category)) in ingredientQuantities)
                {
                    shoppingList.Items.Add(new ShoppingListItem
                    {
                        IngredientId = ingredientId,
                        Quantity = quantity,
                        Unit = unit,
                        Category = category,
                        IsPurchased = false
                    });
                }

                Console.WriteLine($"Created shopping list with {shoppingList.Items.Count} items");

                // Сохраняем список покупок (включая элементы)
                await _shoppingListRepository.AddAsync(shoppingList);
                Console.WriteLine("ShoppingList added to repository");

                var saveResult = await _shoppingListRepository.SaveAllAsync();
                Console.WriteLine($"Save result: {saveResult}");

                if (!saveResult)
                {
                    throw new Exception("Не удалось сохранить список покупок");
                }

                Console.WriteLine($"ShoppingList saved with ID: {shoppingList.Id}");
                Console.WriteLine("=== SUCCESS ===");

                return shoppingList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== ERROR ===");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                throw new Exception($"Ошибка при создании списка покупок: {ex.Message}");
            }
        }
        public async Task<ShoppingList> GetCurrentShoppingListAsync(int userId)
        {
            var shoppingList = await _shoppingListRepository.GetShoppingListByUserIdAsync(userId);
            return shoppingList;
        }

        public async Task<bool> ToggleItemPurchasedAsync(int itemId, bool isPurchased)
        {
            var item = await _shoppingListRepository.GetShoppingListItemAsync(itemId);
            if (item == null) return false;

            item.IsPurchased = isPurchased;
            _shoppingListRepository.Update(item.ShoppingList);
            return await _shoppingListRepository.SaveAllAsync();
        }
    }
    public interface IShoppingListService
    {
        Task<ShoppingList> GenerateShoppingListFromMenuAsync(int menuId, int userId);
        Task<ShoppingList> GetCurrentShoppingListAsync(int userId);
        Task<bool> ToggleItemPurchasedAsync(int itemId, bool isPurchased);
    }
}

