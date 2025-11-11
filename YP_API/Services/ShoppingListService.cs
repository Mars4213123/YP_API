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
                Console.WriteLine($"Menu exists: {menu != null}");

                if (menu == null)
                {
                    Console.WriteLine("Menu not found");
                    return new ShoppingList
                    {
                        Id = 0,
                        Name = "Menu not found",
                        IsCompleted = false,
                        Items = new List<ShoppingListItem>()
                    };
                }

                var shoppingList = new ShoppingList
                {
                    MenuId = menuId,
                    Name = $"Shopping List {DateTime.Now:HHmmss}",
                    CreatedAt = DateTime.UtcNow,
                    IsCompleted = false
                };

                Console.WriteLine($"Created shopping list: MenuId={shoppingList.MenuId}, Name={shoppingList.Name}");

                shoppingList.Items = new List<ShoppingListItem>
                {
                    new ShoppingListItem
                    {
                        IngredientId = 1,
                        Quantity = 1,
                        Unit = "רע",
                        Category = "Ìÿסמ",
                        IsPurchased = false
                    }
                };

                Console.WriteLine($"Added {shoppingList.Items.Count} items");

                await _shoppingListRepository.AddAsync(shoppingList);
                Console.WriteLine("ShoppingList added to repository");

                var saveResult = await _shoppingListRepository.SaveAllAsync();
                Console.WriteLine($"Save result: {saveResult}");

                if (!saveResult)
                {
                    Console.WriteLine("Save failed");
                    throw new Exception("Failed to save shopping list");
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
                Console.WriteLine($"Inner Exception Type: {ex.InnerException?.GetType()}");
                Console.WriteLine($"Inner Stack Trace: {ex.InnerException?.StackTrace}");

                Exception current = ex;
                int depth = 0;
                while (current != null && depth < 5)
                {
                    Console.WriteLine($"Exception depth {depth}: {current.GetType()} - {current.Message}");
                    current = current.InnerException;
                    depth++;
                }

                return new ShoppingList
                {
                    Id = 0,
                    Name = $"Error: {ex.Message}",
                    IsCompleted = false,
                    Items = new List<ShoppingListItem>()
                };
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

