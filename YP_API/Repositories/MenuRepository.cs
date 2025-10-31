using Microsoft.EntityFrameworkCore;
using YP_API.Data;
using YP_API.Interfaces;
using YP_API.Models;

namespace YP_API.Repositories
{
    public class MenuRepository : Repository<WeeklyMenu>, IMenuRepository
    {
        public MenuRepository(RecipePlannerContext context) : base(context) { }

        public async Task<WeeklyMenu> GetCurrentMenuAsync(int userId)
        {
            var today = DateTime.Today;

            return await _context.WeeklyMenus
                .Include(wm => wm.MenuMeals)
                    .ThenInclude(mm => mm.Recipe)
                        .ThenInclude(r => r.RecipeIngredients)
                            .ThenInclude(ri => ri.Ingredient)
                .Include(wm => wm.ShoppingList)
                    .ThenInclude(sl => sl.Items)
                        .ThenInclude(sli => sli.Ingredient)
                .Where(wm => wm.UserId == userId && wm.StartDate <= today && wm.EndDate >= today)
                .OrderByDescending(wm => wm.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<WeeklyMenu>> GetUserMenusAsync(int userId)
        {
            return await _context.WeeklyMenus
                .Include(wm => wm.MenuMeals)
                    .ThenInclude(mm => mm.Recipe)
                .Where(wm => wm.UserId == userId)
                .OrderByDescending(wm => wm.CreatedAt)
                .Take(10)
                .ToListAsync();
        }

        public async Task<WeeklyMenu> CreateMenuAsync(WeeklyMenu menu)
        {
            await _context.WeeklyMenus.AddAsync(menu);
            return menu;
        }

        public async Task<WeeklyMenu> GetMenuWithDetailsAsync(int menuId)
        {
            return await _context.WeeklyMenus
                .Include(wm => wm.MenuMeals)
                    .ThenInclude(mm => mm.Recipe)
                        .ThenInclude(r => r.RecipeIngredients)
                            .ThenInclude(ri => ri.Ingredient)
                .Include(wm => wm.ShoppingList)
                    .ThenInclude(sl => sl.Items)
                        .ThenInclude(sli => sli.Ingredient)
                .FirstOrDefaultAsync(wm => wm.Id == menuId);
        }
    }
}
