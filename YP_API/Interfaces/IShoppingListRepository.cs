using YP_API.Models;

namespace YP_API.Interfaces
{
    public interface IShoppingListRepository : IRepository<ShoppingList>
    {
        Task<ShoppingList> GetCurrentShoppingListAsync(int userId);
        Task<ShoppingList> GetShoppingListWithItemsAsync(int listId);
        Task<ShoppingListItem> GetShoppingListItemAsync(int itemId);
        Task<ShoppingList> GetShoppingListByUserIdAsync(int userId);
    }
}

