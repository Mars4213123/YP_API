using Microsoft.EntityFrameworkCore;
using YP_API.Data;
using YP_API.Interfaces;
using YP_API.Models;

namespace YP_API.Repositories
{
    public class MenuRepository : Repository<MenuPlan>, IMenuRepository
    {
        public MenuRepository(RecipePlannerContext context) : base(context) { }

        public async Task<MenuPlan> GetCurrentMenuAsync(int userId)
        {
            var today = DateTime.Today;

            return await _context.MenuPlans
                .Include(mp => mp.MenuPlanItems)
                    .ThenInclude(mpi => mpi.Recipe)
                        .ThenInclude(r => r.RecipeIngredients)
                            .ThenInclude(ri => ri.Ingredient)
                .Include(mp => mp.ShoppingList)
                    .ThenInclude(sl => sl.Items)
                        .ThenInclude(sli => sli.Ingredient)
                .Where(mp => mp.UserId == userId && mp.StartDate <= today && mp.EndDate >= today)
                .OrderByDescending(mp => mp.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<MenuPlan>> GetUserMenusAsync(int userId)
        {
            return await _context.MenuPlans
                .Include(mp => mp.MenuPlanItems)
                    .ThenInclude(mpi => mpi.Recipe)
                .Where(mp => mp.UserId == userId)
                .OrderByDescending(mp => mp.CreatedAt)
                .ToListAsync();
        }

        public async Task<MenuPlan> CreateMenuAsync(MenuPlan menuPlan)
        {
            await _context.MenuPlans.AddAsync(menuPlan);
            return menuPlan;
        }
    }
}
