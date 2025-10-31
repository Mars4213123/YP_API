using YP_API.Helpers;
using YP_API.Models;

namespace YP_API.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task<bool> SaveAllAsync();
    }

    public interface IRecipeRepository : IRepository<Recipe>
    {
        Task<PagedList<Recipe>> GetRecipesAsync(RecipeSearchParams searchParams);
        Task<IEnumerable<Recipe>> GetRecipesByIdsAsync(IEnumerable<int> ids);
        Task<IEnumerable<Recipe>> GetRecipesByAllergiesAsync(IEnumerable<string> allergyCodes);
        Task AddRecipeAsync(Recipe recipe);
    }

    public interface IIngredientRepository : IRepository<Ingredient>
    {
        Task<PagedList<Ingredient>> GetIngredientsAsync(IngredientSearchParams searchParams);
        Task<IEnumerable<Ingredient>> SearchIngredientsAsync(string query, int? categoryId = null);
        Task<Ingredient> GetIngredientByNameAsync(string name);
        Task<IEnumerable<IngredientCategory>> GetCategoriesAsync();
        Task<IngredientCategory> GetCategoryAsync(int id);
    }

    public interface IMenuRepository : IRepository<MenuPlan>
    {
        Task<MenuPlan> GetCurrentMenuAsync(int userId);
        Task<IEnumerable<MenuPlan>> GetUserMenusAsync(int userId);
        Task<MenuPlan> CreateMenuAsync(MenuPlan menuPlan);
    }

    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetUserByUsernameAsync(string username);
        Task<User> GetUserByEmailAsync(string email);
        Task<IEnumerable<Allergy>> GetUserAllergiesAsync(int userId);
        Task<bool> UserExistsAsync(string username, string email);
    }
}
