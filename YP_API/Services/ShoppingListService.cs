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
                var menu = await _menuRepository.GetByIdAsync(menuId);
                if (menu == null)
                {
                    throw new Exception($"Меню с ID {menuId} не найдено");
                }

                if (menu.UserId != userId)
                {
                    throw new Exception("Меню не принадлежит пользователю");
                }

                var menuWithDetails = await _menuRepository.GetMenuWithDetailsAsync(menuId);
                if (menuWithDetails == null)
                {
                    throw new Exception("Не удалось загрузить детали меню");
                }

                var shoppingList = new ShoppingList
                {
                    MenuId = menuId,
                    UserId = userId,
                    Name = $"Список покупок для {menu.Name}",
                    CreatedAt = DateTime.UtcNow,
                    IsCompleted = false,
                    Items = new List<ShoppingListItem>()
                };

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
                            var existing = ingredientQuantities[ingredient.Id];
                            ingredientQuantities[ingredient.Id] = (
                                existing.Quantity + recipeIngredient.Quantity,
                                existing.Unit,
                                existing.Category
                            );
                        }
                        else
                        {
                            ingredientQuantities[ingredient.Id] = (
                                recipeIngredient.Quantity,
                                recipeIngredient.Unit,
                                ingredient.Category
                            );
                        }
                    }
                }

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

                await _shoppingListRepository.AddAsync(shoppingList);

                var saveResult = await _shoppingListRepository.SaveAllAsync();

                if (!saveResult)
                {
                    throw new Exception("Не удалось сохранить список покупок");
                }

                return shoppingList;
            }
            catch (Exception ex)
            {
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