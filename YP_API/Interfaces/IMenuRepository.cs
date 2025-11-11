using YP_API.Models;

namespace YP_API.Interfaces
{
    public interface IMenuRepository : IRepository<WeeklyMenu>
    {
        Task<WeeklyMenu> GetCurrentMenuAsync(int userId);
        Task<IEnumerable<WeeklyMenu>> GetUserMenusAsync(int userId);
        Task<WeeklyMenu> CreateMenuAsync(WeeklyMenu menu);
        Task<WeeklyMenu> GetMenuWithDetailsAsync(int menuId);
    }
}

