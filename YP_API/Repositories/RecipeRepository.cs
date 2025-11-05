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

        public async Task<bool> ToggleFavoriteAsync(int userId, int recipeId)
        {
            try
            {
                Console.WriteLine($"ToggleFavoriteAsync: UserId={userId}, RecipeId={recipeId}");

                // Проверяем существование рецепта
                var recipe = await _context.Recipes.FindAsync(recipeId);
                if (recipe == null)
                {
                    Console.WriteLine("Recipe not found");
                    throw new Exception("Recipe not found");
                }

                // Проверяем существование пользователя
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    Console.WriteLine("User not found");
                    throw new Exception("User not found");
                }

                // Ищем существующий избранный рецепт
                var existingFavorite = await _context.UserFavorites
                    .FirstOrDefaultAsync(uf => uf.UserId == userId && uf.RecipeId == recipeId);

                if (existingFavorite != null)
                {
                    // Удаляем из избранного
                    Console.WriteLine("Removing from favorites");
                    _context.UserFavorites.Remove(existingFavorite);
                }
                else
                {
                    // Добавляем в избранное
                    Console.WriteLine("Adding to favorites");
                    var favorite = new UserFavorite
                    {
                        UserId = userId,
                        RecipeId = recipeId,
                        AddedAt = DateTime.UtcNow
                    };
                    await _context.UserFavorites.AddAsync(favorite);
                }

                // Сохраняем изменения
                var result = await _context.SaveChangesAsync() > 0;
                Console.WriteLine($"Save result: {result}");
                return result;
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"Database error in ToggleFavoriteAsync: {dbEx.Message}");
                Console.WriteLine($"Inner exception: {dbEx.InnerException?.Message}");
                throw new Exception("Database error occurred while updating favorites");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ToggleFavoriteAsync: {ex.Message}");
                Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
                throw new Exception("Error occurred while updating favorites");
            }
        }

        public async Task<PagedList<Recipe>> GetRecipesAsync(RecipeSearchParams searchParams)
        {
            var query = _context.Recipes
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchParams.Name))
                query = query.Where(r => r.Title.Contains(searchParams.Name));

            if (searchParams.Tags.Any())
            {
                var allRecipes = await query.ToListAsync();
                var filteredRecipes = allRecipes.Where(r => searchParams.Tags.All(t => r.Tags.Contains(t))).ToList();

                return PagedList<Recipe>.Create(
                    filteredRecipes,
                    searchParams.PageNumber,
                    searchParams.PageSize
                );
            }

            if (searchParams.ExcludedAllergens.Any())
            {
                var allRecipes = await query.ToListAsync();
                var filteredRecipes = allRecipes.Where(r => !r.Allergens.Any(a => searchParams.ExcludedAllergens.Contains(a))).ToList();

                return PagedList<Recipe>.Create(
                    filteredRecipes,
                    searchParams.PageNumber,
                    searchParams.PageSize
                );
            }

            if (searchParams.MaxPrepTime.HasValue)
                query = query.Where(r => r.PrepTime <= searchParams.MaxPrepTime.Value);

            if (searchParams.MaxCalories.HasValue)
                query = query.Where(r => r.Calories <= searchParams.MaxCalories.Value);

            if (searchParams.CuisineTypes.Any())
            {
                var allRecipes = await query.ToListAsync();
                var filteredRecipes = allRecipes.Where(r => searchParams.CuisineTypes.Contains(r.CuisineType)).ToList();

                return PagedList<Recipe>.Create(
                    filteredRecipes,
                    searchParams.PageNumber,
                    searchParams.PageSize
                );
            }

            query = searchParams.SortBy?.ToLower() switch
            {
                "calories" => searchParams.SortDescending
                    ? query.OrderByDescending(r => r.Calories)
                    : query.OrderBy(r => r.Calories),
                "time" => searchParams.SortDescending
                    ? query.OrderByDescending(r => r.PrepTime + r.CookTime)
                    : query.OrderBy(r => r.PrepTime + r.CookTime),
                "difficulty" => searchParams.SortDescending
                    ? query.OrderByDescending(r => r.Difficulty)
                    : query.OrderBy(r => r.Difficulty),
                _ => searchParams.SortDescending
                    ? query.OrderByDescending(r => r.Title)
                    : query.OrderBy(r => r.Title)
            };

            var totalCount = await query.CountAsync();
            var recipes = await query
                .Skip((searchParams.PageNumber - 1) * searchParams.PageSize)
                .Take(searchParams.PageSize)
                .ToListAsync();

            return new PagedList<Recipe>(recipes, totalCount, searchParams.PageNumber, searchParams.PageSize);
        }

        public async Task<Recipe> GetRecipeWithDetailsAsync(int id)
        {
            return await _context.Recipes
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<Recipe>> GetRecipesForMenuAsync(List<string> excludedAllergens, List<string> cuisineTags, decimal? maxCalories)
        {
            var recipes = await _context.Recipes
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .Where(r => r.IsPublic)
                .ToListAsync();

            if (excludedAllergens != null && excludedAllergens.Any())
            {
                recipes = recipes.Where(r => !r.Allergens.Any(a => excludedAllergens.Contains(a))).ToList();
            }

            if (cuisineTags != null && cuisineTags.Any())
            {
                recipes = recipes.Where(r => cuisineTags.Contains(r.CuisineType)).ToList();
            }

            if (maxCalories.HasValue)
            {
                recipes = recipes.Where(r => r.Calories <= maxCalories.Value).ToList();
            }

            return recipes;
        }

        public async Task<IEnumerable<Recipe>> GetUserFavoritesAsync(int userId)
        {
            return await _context.UserFavorites
                .Where(uf => uf.UserId == userId)
                .Include(uf => uf.Recipe)
                    .ThenInclude(r => r.RecipeIngredients)
                        .ThenInclude(ri => ri.Ingredient)
                .Select(uf => uf.Recipe)
                .ToListAsync();
        }

        public async Task<IEnumerable<Recipe>> SearchRecipesAsync(string query, List<string> tags, List<string> excludedAllergens)
        {
            var searchQuery = _context.Recipes
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .AsQueryable();

            if (!string.IsNullOrEmpty(query))
            {
                searchQuery = searchQuery.Where(r =>
                    r.Title.Contains(query) ||
                    r.Description.Contains(query) ||
                    r.Tags.Any(t => t.Contains(query)) ||
                    r.RecipeIngredients.Any(ri => ri.Ingredient.Name.Contains(query))
                );
            }

            if (tags != null && tags.Any())
                searchQuery = searchQuery.Where(r => tags.All(t => r.Tags.Contains(t)));

            if (excludedAllergens != null && excludedAllergens.Any())
                searchQuery = searchQuery.Where(r => !r.Allergens.Any(a => excludedAllergens.Contains(a)));

            return await searchQuery.Take(50).ToListAsync();
        }
    }
}

