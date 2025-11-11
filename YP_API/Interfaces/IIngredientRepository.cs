using YP_API.Helpers;
using YP_API.Models;

namespace YP_API.Interfaces
{
    public interface IIngredientRepository : IRepository<Ingredient>
    {
        Task<PagedList<Ingredient>> GetIngredientsAsync(IngredientSearchParams searchParams);
        Task<IEnumerable<Ingredient>> SearchIngredientsAsync(string query);
        Task<IEnumerable<string>> GetCategoriesAsync();
        Task<Ingredient> GetIngredientByNameAsync(string name);
    }
}

