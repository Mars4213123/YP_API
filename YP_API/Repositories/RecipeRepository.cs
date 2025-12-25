using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YP_API.Data;
using YP_API.Helpers;
using YP_API.Interfaces;
using YP_API.Models;

namespace YP_API.Repositories
{
    public class RecipeRepository : Repository<Recipe>, IRecipeRepository
    {
        private readonly ILogger<RecipeRepository> _logger;

        public RecipeRepository(RecipePlannerContext context, ILogger<RecipeRepository> logger) : base(context)
        {
            _logger = logger;
        }

        public async Task<bool> ToggleFavoriteAsync(int userId, int recipeId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _logger.LogInformation($"ToggleFavoriteAsync: UserId={userId}, RecipeId={recipeId}");

                var recipe = await _context.Recipes.FindAsync(recipeId);
                if (recipe == null)
                {
                    _logger.LogWarning($"Recipe {recipeId} not found");
                    throw new Exception($"Рецепт с ID {recipeId} не найден");
                }

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning($"User {userId} not found");
                    throw new Exception($"Пользователь с ID {userId} не найден");
                }

                var existingFavorite = await _context.UserFavorites
                    .AsNoTracking()
                    .FirstOrDefaultAsync(uf => uf.UserId == userId && uf.RecipeId == recipeId);

                if (existingFavorite != null)
                {
                    _logger.LogInformation($"Removing recipe {recipeId} from favorites for user {userId}");
                    _context.UserFavorites.Remove(existingFavorite);
                }
                else
                {
                    _logger.LogInformation($"Adding recipe {recipeId} to favorites for user {userId}");
                    var favorite = new UserFavorite
                    {
                        UserId = userId,
                        RecipeId = recipeId,
                        AddedAt = DateTime.UtcNow
                    };
                    await _context.UserFavorites.AddAsync(favorite);
                }

                var result = await _context.SaveChangesAsync() > 0;

                if (result)
                {
                    await transaction.CommitAsync();
                    _logger.LogInformation($"Favorite toggled successfully for user {userId}, recipe {recipeId}");
                }
                else
                {
                    await transaction.RollbackAsync();
                    _logger.LogWarning($"Failed to save favorite changes for user {userId}, recipe {recipeId}");
                }

                return result;
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();
                _logger.LogError($"Database error in ToggleFavoriteAsync: {dbEx.Message}");
                _logger.LogError($"Inner exception: {dbEx.InnerException?.Message}");

                if (dbEx.InnerException?.Message?.Contains("foreign key constraint") == true)
                {
                    throw new Exception("Ошибка связи с базой данных. Проверьте существование рецепта и пользователя.");
                }

                throw new Exception("Ошибка базы данных при изменении избранного");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError($"Error in ToggleFavoriteAsync: {ex.Message}");
                _logger.LogError($"Inner exception: {ex.InnerException?.Message}");
                throw new Exception($"Ошибка при изменении избранного: {ex.Message}");
            }
        }

        public async Task<bool> IsRecipeFavoriteAsync(int userId, int recipeId)
        {
            return await _context.UserFavorites
                .AnyAsync(uf => uf.UserId == userId && uf.RecipeId == recipeId);
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

            if (!string.IsNullOrEmpty(searchParams.Difficulty) &&
                !string.IsNullOrWhiteSpace(searchParams.Difficulty) &&
                searchParams.Difficulty.ToLower() != "все" &&
                searchParams.Difficulty.ToLower() != "all")
            {
                query = query.Where(r => r.Difficulty.ToLower() == searchParams.Difficulty.ToLower());
            }

            if (searchParams.MaxPrepTime.HasValue)
                query = query.Where(r => r.PrepTime <= searchParams.MaxPrepTime.Value);

            if (searchParams.MaxCookTime.HasValue)
                query = query.Where(r => r.CookTime <= searchParams.MaxCookTime.Value);

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
            var allRecipes = await _context.Recipes
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .Where(r => r.IsPublic)
                .ToListAsync();

            var filteredRecipes = allRecipes.AsEnumerable();

            if (excludedAllergens != null && excludedAllergens.Any())
            {
                filteredRecipes = filteredRecipes
                    .Where(r => !r.Allergens.Any(a => excludedAllergens.Contains(a)));
            }

            if (cuisineTags != null && cuisineTags.Any())
            {
                filteredRecipes = filteredRecipes
                    .Where(r => cuisineTags.Contains(r.CuisineType));
            }

            if (maxCalories.HasValue)
            {
                filteredRecipes = filteredRecipes
                    .Where(r => r.Calories <= maxCalories.Value);
            }

            return filteredRecipes.ToList();
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