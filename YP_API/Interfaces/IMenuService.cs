using System.Threading.Tasks;

namespace YP_API.Interfaces
{
    public interface IMenuService
    {
        // Generate a weekly menu for the user based on their inventory. Returns created menu id or null.
        Task<int?> GenerateMenuFromInventoryAsync(int userId);
    }
}
