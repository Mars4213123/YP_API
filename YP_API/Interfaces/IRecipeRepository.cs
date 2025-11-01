using YP_API.DTOs;
using YP_API.Helpers;
using YP_API.Models;

namespace YP_API.Interfaces
{
    public interface IRecipeRepository : IRepository<Recipe>
    {
        Task<PagedList<RecipeDto>> GetRecipesAsync(RecipeSearchParams searchParams);
        Task<RecipeDto> GetRecipeDtoByIdAsync(int id);
        Task<IEnumerable<Recipe>> GetRecipesForMenuAsync(List<string> excludedAllergens, List<string> cuisineTags, decimal? maxCalories);
        Task<Recipe> CreateRecipeAsync(CreateRecipeDto createRecipeDto);
        Task<bool> ToggleFavoriteAsync(int userId, int recipeId);
        Task<IEnumerable<RecipeDto>> GetUserFavoritesAsync(int userId);
        Task<IEnumerable<Recipe>> SearchRecipesAsync(string query, List<string> tags, List<string> excludedAllergens);
    }
}

