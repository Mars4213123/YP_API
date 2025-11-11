using Microsoft.EntityFrameworkCore;
using YP_API.Data;
using YP_API.Helpers;
using YP_API.Interfaces;
using YP_API.Models;

namespace YP_API.Repositories
{
    public class IngredientRepository : Repository<Ingredient>, IIngredientRepository
    {
        public IngredientRepository(RecipePlannerContext context) : base(context) { }

        public async Task<PagedList<Ingredient>> GetIngredientsAsync(IngredientSearchParams searchParams)
        {
            var query = _context.Ingredients.AsQueryable();

            if (!string.IsNullOrEmpty(searchParams.Name))
                query = query.Where(i => i.Name.Contains(searchParams.Name));

            if (!string.IsNullOrEmpty(searchParams.Category))
                query = query.Where(i => i.Category == searchParams.Category);

            if (searchParams.HasAllergens.HasValue)
                query = searchParams.HasAllergens.Value
                    ? query.Where(i => i.Allergens.Any())
                    : query.Where(i => !i.Allergens.Any());

            query = searchParams.SortBy?.ToLower() switch
            {
                "category" => searchParams.SortDescending
                    ? query.OrderByDescending(i => i.Category)
                    : query.OrderBy(i => i.Category),
                _ => searchParams.SortDescending
                    ? query.OrderByDescending(i => i.Name)
                    : query.OrderBy(i => i.Name)
            };

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((searchParams.PageNumber - 1) * searchParams.PageSize)
                .Take(searchParams.PageSize)
                .ToListAsync();

            return new PagedList<Ingredient>(items, totalCount, searchParams.PageNumber, searchParams.PageSize);
        }

        public async Task<IEnumerable<Ingredient>> SearchIngredientsAsync(string query)
        {
            return await _context.Ingredients
                .Where(i => i.Name.Contains(query))
                .OrderBy(i => i.Name)
                .Take(20)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetCategoriesAsync()
        {
            return await _context.Ingredients
                .Select(i => i.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<Ingredient> GetIngredientByNameAsync(string name)
        {
            return await _context.Ingredients
                .FirstOrDefaultAsync(i => i.Name == name);
        }
    }
}

