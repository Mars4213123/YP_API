using Microsoft.EntityFrameworkCore;
using YP_API.Data;
using YP_API.DTOs;
using YP_API.Helpers;
using YP_API.Interfaces;
using YP_API.Models;

namespace YP_API.Repositories
{
    public class RecipeRepository : Repository<Recipe>, IRecipeRepository
    {
        public RecipeRepository(RecipePlannerContext context) : base(context) { }

        public async Task<PagedList<RecipeDto>> GetRecipesAsync(RecipeSearchParams searchParams)
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

                return PagedList<RecipeDto>.Create(
                    filteredRecipes.Select(r => MapToRecipeDto(r)),
                    searchParams.PageNumber,
                    searchParams.PageSize
                );
            }

            if (searchParams.ExcludedAllergens.Any())
            {
                var allRecipes = await query.ToListAsync();
                var filteredRecipes = allRecipes.Where(r => !r.Allergens.Any(a => searchParams.ExcludedAllergens.Contains(a))).ToList();

                return PagedList<RecipeDto>.Create(
                    filteredRecipes.Select(r => MapToRecipeDto(r)),
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

                return PagedList<RecipeDto>.Create(
                    filteredRecipes.Select(r => MapToRecipeDto(r)),
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

            var recipeDtos = recipes.Select(r => MapToRecipeDto(r)).ToList();
            return new PagedList<RecipeDto>(recipeDtos, totalCount, searchParams.PageNumber, searchParams.PageSize);
        }

        public async Task<RecipeDto> GetRecipeDtoByIdAsync(int id)
        {
            var recipe = await _context.Recipes
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .FirstOrDefaultAsync(r => r.Id == id);

            return recipe != null ? MapToRecipeDto(recipe) : null;
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

        public async Task<Recipe> CreateRecipeAsync(CreateRecipeDto createRecipeDto)
        {
            var recipe = new Recipe
            {
                Title = createRecipeDto.Title,
                Description = createRecipeDto.Description,
                Instructions = createRecipeDto.Instructions,
                PrepTime = createRecipeDto.PrepTime,
                CookTime = createRecipeDto.CookTime,
                Servings = createRecipeDto.Servings,
                Calories = createRecipeDto.Calories,
                ImageUrl = createRecipeDto.ImageUrl,
                Tags = createRecipeDto.Tags ?? new List<string>(),
                Allergens = createRecipeDto.Allergens ?? new List<string>(),
                CuisineType = createRecipeDto.CuisineType,
                Difficulty = createRecipeDto.Difficulty,
                CreatedAt = DateTime.UtcNow
            };

            if (createRecipeDto.Ingredients != null)
            {
                foreach (var ingredientDto in createRecipeDto.Ingredients)
                {
                    recipe.RecipeIngredients.Add(new RecipeIngredient
                    {
                        IngredientId = ingredientDto.IngredientId,
                        Quantity = ingredientDto.Quantity,
                        Unit = ingredientDto.Unit
                    });
                }
            }

            await _context.Recipes.AddAsync(recipe);
            return recipe;
        }

        public async Task<bool> ToggleFavoriteAsync(int userId, int recipeId)
        {
            var existingFavorite = await _context.UserFavorites
                .FirstOrDefaultAsync(uf => uf.UserId == userId && uf.RecipeId == recipeId);

            if (existingFavorite != null)
            {
                _context.UserFavorites.Remove(existingFavorite);
            }
            else
            {
                var favorite = new UserFavorite
                {
                    UserId = userId,
                    RecipeId = recipeId,
                    AddedAt = DateTime.UtcNow
                };
                await _context.UserFavorites.AddAsync(favorite);
            }

            return await SaveAllAsync();
        }

        public async Task<IEnumerable<RecipeDto>> GetUserFavoritesAsync(int userId)
        {
            var favoriteRecipes = await _context.UserFavorites
                .Where(uf => uf.UserId == userId)
                .Include(uf => uf.Recipe)
                    .ThenInclude(r => r.RecipeIngredients)
                        .ThenInclude(ri => ri.Ingredient)
                .Select(uf => uf.Recipe)
                .ToListAsync();

            return favoriteRecipes.Select(r => MapToRecipeDto(r));
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

        private RecipeDto MapToRecipeDto(Recipe recipe)
        {
            return new RecipeDto
            {
                Id = recipe.Id,
                Title = recipe.Title ?? string.Empty,
                Description = recipe.Description ?? string.Empty,
                Instructions = recipe.Instructions ?? string.Empty,
                PrepTime = recipe.PrepTime,
                CookTime = recipe.CookTime,
                Servings = recipe.Servings,
                Calories = recipe.Calories,
                ImageUrl = recipe.ImageUrl ?? string.Empty,
                Tags = recipe.Tags ?? new List<string>(),
                Allergens = recipe.Allergens ?? new List<string>(),
                CuisineType = recipe.CuisineType ?? string.Empty,
                Difficulty = recipe.Difficulty ?? string.Empty,
                Ingredients = recipe.RecipeIngredients?.Select(ri => new RecipeIngredientDto
                {
                    IngredientId = ri.IngredientId,
                    IngredientName = ri.Ingredient?.Name ?? string.Empty,
                    Category = ri.Ingredient?.Category ?? string.Empty,
                    Quantity = ri.Quantity,
                    Unit = ri.Unit ?? string.Empty
                }).ToList() ?? new List<RecipeIngredientDto>(),
                IsFavorite = false
            };
        }
    }
}
