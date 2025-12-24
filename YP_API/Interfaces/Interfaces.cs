using YP_API.Models;

namespace YP_API.Interfaces
{
    public interface IRecipeRepository
    {
        Task<IEnumerable<Recipe>> GetAllAsync();
        Task<Recipe> GetByIdAsync(int id);
        Task<bool> ToggleFavoriteAsync(int userId, int recipeId);
        Task<IEnumerable<Recipe>> GetUserFavoritesAsync(int userId);
    }

    public interface IUserRepository
    {
        Task<User> GetUserByUsernameAsync(string username);
        Task AddAsync(User entity);
        Task<bool> SaveAllAsync();
    }
}