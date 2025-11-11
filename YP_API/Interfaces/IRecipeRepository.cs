using YP_API.Helpers;
using YP_API.Models;

namespace YP_API.Interfaces
{
    public interface IRecipeRepository : IRepository<Recipe>
    {
        Task<PagedList<Recipe>> GetRecipesAsync(RecipeSearchParams searchParams);
        Task<Recipe> GetRecipeWithDetailsAsync(int id);
        Task<IEnumerable<Recipe>> GetRecipesForMenuAsync(List<string> excludedAllergens, List<string> cuisineTags, decimal? maxCalories);
        Task<bool> ToggleFavoriteAsync(int userId, int recipeId);
        Task<bool> IsRecipeFavoriteAsync(int userId, int recipeId);
        Task<IEnumerable<Recipe>> GetUserFavoritesAsync(int userId);
        Task<IEnumerable<Recipe>> SearchRecipesAsync(string query, List<string> tags, List<string> excludedAllergens);
    }
}