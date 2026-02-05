namespace YP_API.Interfaces
{
    public interface IMenuService
    {
        Task<int?> GenerateMenuFromInventoryAsync(int userId);
    }
}
