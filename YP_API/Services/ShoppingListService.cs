using YP_API.DTOs;
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

        public ShoppingListService(IShoppingListRepository shoppingListRepository, IMenuRepository menuRepository, IIngredientRepository ingredientRepository)
        {
            _shoppingListRepository = shoppingListRepository;
            _menuRepository = menuRepository;
            _ingredientRepository = ingredientRepository;
        }

        public async Task<ShoppingListDto> GenerateShoppingListFromMenuAsync(int menuId, int userId)
        {
            var menu = await _menuRepository.GetMenuWithDetailsAsync(menuId);
            if (menu == null) throw new Exception("Menu not found");

            var existingList = menu.ShoppingList;
            if (existingList != null)
            {
                _shoppingListRepository.Delete(existingList);
                await _shoppingListRepository.SaveAllAsync();
            }

            var ingredientQuantities = new Dictionary<int, (decimal Quantity, string Unit, string Category)>();

            foreach (var meal in menu.MenuMeals)
            {
                foreach (var recipeIngredient in meal.Recipe.RecipeIngredients)
                {
                    var ingredient = recipeIngredient.Ingredient;
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

            var shoppingList = new ShoppingList
            {
                MenuId = menuId,
                Name = $"Shopping List for {menu.Name}",
                CreatedAt = DateTime.UtcNow
            };

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
            await _shoppingListRepository.SaveAllAsync();

            return await GetShoppingListDtoAsync(shoppingList.Id);
        }

        public async Task<ShoppingListDto> GetCurrentShoppingListAsync(int userId)
        {
            var shoppingList = await _shoppingListRepository.GetCurrentShoppingListAsync(userId);
            return shoppingList != null ? await GetShoppingListDtoAsync(shoppingList.Id) : null;
        }

        public async Task<bool> ToggleItemPurchasedAsync(int itemId, bool isPurchased)
        {
            var item = await _shoppingListRepository.GetShoppingListItemAsync(itemId);
            if (item == null) return false;

            item.IsPurchased = isPurchased;
            _shoppingListRepository.Update(item.ShoppingList);
            return await _shoppingListRepository.SaveAllAsync();
        }

        private async Task<ShoppingListDto> GetShoppingListDtoAsync(int listId)
        {
            var shoppingList = await _shoppingListRepository.GetShoppingListWithItemsAsync(listId);
            if (shoppingList == null) return null;

            return new ShoppingListDto
            {
                Id = shoppingList.Id,
                Name = shoppingList.Name,
                IsCompleted = shoppingList.IsCompleted,
                Items = shoppingList.Items.Select(item => new ShoppingListItemDto
                {
                    Id = item.Id,
                    IngredientId = item.IngredientId,
                    IngredientName = item.Ingredient.Name,
                    Category = item.Category,
                    Quantity = item.Quantity,
                    Unit = item.Unit,
                    IsPurchased = item.IsPurchased
                }).OrderBy(item => item.Category)
                  .ThenBy(item => item.IngredientName)
                  .ToList()
            };
        }
    }

    public interface IShoppingListService
    {
        Task<ShoppingListDto> GenerateShoppingListFromMenuAsync(int menuId, int userId);
        Task<ShoppingListDto> GetCurrentShoppingListAsync(int userId);
        Task<bool> ToggleItemPurchasedAsync(int itemId, bool isPurchased);
    }
}
