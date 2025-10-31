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
            var query = _context.Ingredients
                .Include(i => i.Category)
                .Include(i => i.IngredientAllergies)
                    .ThenInclude(ia => ia.Allergy)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchParams.Name))
            {
                query = query.Where(i => i.Name.Contains(searchParams.Name));
            }

            if (searchParams.CategoryId.HasValue)
            {
                query = query.Where(i => i.CategoryId == searchParams.CategoryId.Value);
            }

            if (searchParams.HasAllergies.HasValue)
            {
                query = searchParams.HasAllergies.Value
                    ? query.Where(i => i.IngredientAllergies.Any())
                    : query.Where(i => !i.IngredientAllergies.Any());
            }

            query = searchParams.SortBy?.ToLower() switch
            {
                "category" => searchParams.SortDescending
                    ? query.OrderByDescending(i => i.Category.Name)
                    : query.OrderBy(i => i.Category.Name),
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

        public async Task<IEnumerable<Ingredient>> SearchIngredientsAsync(string query, int? categoryId = null)
        {
            var ingredientsQuery = _context.Ingredients
                .Include(i => i.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(query))
            {
                ingredientsQuery = ingredientsQuery.Where(i => i.Name.Contains(query));
            }

            if (categoryId.HasValue)
            {
                ingredientsQuery = ingredientsQuery.Where(i => i.CategoryId == categoryId.Value);
            }

            return await ingredientsQuery
                .OrderBy(i => i.Name)
                .Take(20)
                .ToListAsync();
        }

        public async Task<Ingredient> GetIngredientByNameAsync(string name)
        {
            return await _context.Ingredients
                .FirstOrDefaultAsync(i => i.Name == name);
        }

        public async Task<IEnumerable<IngredientCategory>> GetCategoriesAsync()
        {
            return await _context.IngredientCategories
                .Include(ic => ic.Ingredients)
                .OrderBy(ic => ic.Name)
                .ToListAsync();
        }

        public async Task<IngredientCategory> GetCategoryAsync(int id)
        {
            return await _context.IngredientCategories
                .FirstOrDefaultAsync(ic => ic.Id == id);
        }
    }
}

