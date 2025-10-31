using Microsoft.EntityFrameworkCore;
using YP_API.Data;
using YP_API.Helpers;
using YP_API.Interfaces;
using YP_API.Models;

namespace YP_API.Repositories
{
    public class RecipeRepository : Repository<Recipe>, IRecipeRepository
    {
        public RecipeRepository(RecipePlannerContext context) : base(context) { }

        public async Task<PagedList<Recipe>> GetRecipesAsync(RecipeSearchParams searchParams)
        {
            var query = _context.Recipes
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .Include(r => r.RecipeTags)
                .Include(r => r.RecipeAllergies)
                    .ThenInclude(ra => ra.Allergy)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchParams.Name))
            {
                query = query.Where(r => r.Title.Contains(searchParams.Name));
            }

            if (searchParams.Tags.Any())
            {
                query = query.Where(r => r.RecipeTags.Any(rt => searchParams.Tags.Contains(rt.Tag)));
            }

            if (searchParams.ExcludedAllergies.Any())
            {
                query = query.Where(r => !r.RecipeAllergies.Any(ra =>
                    searchParams.ExcludedAllergies.Contains(ra.Allergy.Code)));
            }

            if (searchParams.MaxPrepTime.HasValue)
            {
                query = query.Where(r => r.PrepTime <= searchParams.MaxPrepTime.Value);
            }

            if (searchParams.MaxCookTime.HasValue)
            {
                query = query.Where(r => r.CookTime <= searchParams.MaxCookTime.Value);
            }

            if (searchParams.MaxCalories.HasValue)
            {
                query = query.Where(r => r.Calories <= searchParams.MaxCalories.Value);
            }

            query = searchParams.SortBy?.ToLower() switch
            {
                "calories" => searchParams.SortDescending
                    ? query.OrderByDescending(r => r.Calories)
                    : query.OrderBy(r => r.Calories),
                "time" => searchParams.SortDescending
                    ? query.OrderByDescending(r => r.PrepTime + r.CookTime)
                    : query.OrderBy(r => r.PrepTime + r.CookTime),
                _ => searchParams.SortDescending
                    ? query.OrderByDescending(r => r.Title)
                    : query.OrderBy(r => r.Title)
            };

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((searchParams.PageNumber - 1) * searchParams.PageSize)
                .Take(searchParams.PageSize)
                .ToListAsync();

            return new PagedList<Recipe>(items, totalCount, searchParams.PageNumber, searchParams.PageSize);
        }

        public async Task<IEnumerable<Recipe>> GetRecipesByIdsAsync(IEnumerable<int> ids)
        {
            return await _context.Recipes
                .Where(r => ids.Contains(r.Id))
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .Include(r => r.RecipeAllergies)
                    .ThenInclude(ra => ra.Allergy)
                .ToListAsync();
        }

        public async Task<IEnumerable<Recipe>> GetRecipesByAllergiesAsync(IEnumerable<string> allergyCodes)
        {
            return await _context.Recipes
                .Where(r => !r.RecipeAllergies.Any(ra => allergyCodes.Contains(ra.Allergy.Code)))
                .Include(r => r.RecipeIngredients)
                .Include(r => r.RecipeAllergies)
                .ToListAsync();
        }

        public async Task AddRecipeAsync(Recipe recipe)
        {
            await _context.Recipes.AddAsync(recipe);
        }
    }
}
