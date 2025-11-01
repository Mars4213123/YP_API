using Microsoft.EntityFrameworkCore;
using YP_API.Data;
using YP_API.Interfaces;
using YP_API.Models;

namespace YP_API.Repositories
{
    public class ShoppingListRepository : Repository<ShoppingList>, IShoppingListRepository
    {
        public ShoppingListRepository(RecipePlannerContext context) : base(context) { }

        public async Task<ShoppingList> GetCurrentShoppingListAsync(int userId)
        {
            return await _context.ShoppingLists
                .Include(sl => sl.WeeklyMenu) 
                .Include(sl => sl.Items)
                    .ThenInclude(sli => sli.Ingredient)
                .Where(sl => sl.MenuId != null &&
                           sl.WeeklyMenu != null &&
                           sl.WeeklyMenu.UserId == userId && 
                           !sl.IsCompleted)
                .OrderByDescending(sl => sl.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<ShoppingList> GetShoppingListWithItemsAsync(int listId)
        {
            return await _context.ShoppingLists
                .Include(sl => sl.Items)
                    .ThenInclude(sli => sli.Ingredient)
                .FirstOrDefaultAsync(sl => sl.Id == listId);
        }

        public async Task<ShoppingListItem> GetShoppingListItemAsync(int itemId)
        {
            return await _context.ShoppingListItems
                .Include(sli => sli.ShoppingList)
                .FirstOrDefaultAsync(sli => sli.Id == itemId);
        }

        public async Task<ShoppingList> GetShoppingListByUserIdAsync(int userId)
        {
            try
            {
                return await _context.ShoppingLists
                    .Include(sl => sl.WeeklyMenu)
                    .Include(sl => sl.Items)
                        .ThenInclude(sli => sli.Ingredient)
                    .Where(sl => sl.WeeklyMenu != null && sl.WeeklyMenu.UserId == userId)
                    .OrderByDescending(sl => sl.CreatedAt)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"Error in GetShoppingListByUserIdAsync: {ex.Message}");
                return null;
            }
        }
    }
}

