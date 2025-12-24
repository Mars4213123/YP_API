using Microsoft.EntityFrameworkCore;
using YP_API.Data;
using YP_API.Interfaces;
using YP_API.Models;

namespace YP_API.Repositories
{
    public class RecipeRepository : IRecipeRepository
    {
        private readonly RecipePlannerContext _context;

        public RecipeRepository(RecipePlannerContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Recipe>> GetAllAsync()
        {
            return await _context.Recipes.ToListAsync();
        }

        public async Task<Recipe> GetByIdAsync(int id)
        {
            return await _context.Recipes.FindAsync(id);
        }

        public async Task<bool> ToggleFavoriteAsync(int userId, int recipeId)
        {
            var existing = await _context.UserFavorites
                .FirstOrDefaultAsync(uf => uf.UserId == userId && uf.RecipeId == recipeId);

            if (existing != null)
            {
                _context.UserFavorites.Remove(existing);
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

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<Recipe>> GetUserFavoritesAsync(int userId)
        {
            return await _context.UserFavorites
                .Where(uf => uf.UserId == userId)
                .Include(uf => uf.Recipe)
                .Select(uf => uf.Recipe)
                .ToListAsync();
        }
    }
}