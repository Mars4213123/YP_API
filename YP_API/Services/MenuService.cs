using System.Text;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using YP_API.Data;
using YP_API.Interfaces;
using YP_API.Models;
using YP_API.Models.AIAPI.Responce;
using YP_API.Models.AIAPI;

namespace YP_API.Services
{
    public class MenuService : IMenuService
    {
        private readonly RecipePlannerContext _context;
        private readonly Random _rnd = new Random();

        public MenuService(RecipePlannerContext context)
        {
            _context = context;
        }

        public async Task<int?> GenerateMenuFromInventoryAsync(int userId)
        {
            // Get user's inventory ingredient ids
            var ingredientIds = await _context.UserInventories
                .Where(ui => ui.UserId == userId)
                .Select(ui => ui.IngredientId)
                .ToListAsync();

            if (!ingredientIds.Any())
                return null;

            // Exclude recipes that contain ingredients the user is allergic to
            var allergyIds = await _context.UserAllergies
                .Where(a => a.UserId == userId)
                .Select(a => a.IngredientId)
                .ToListAsync();

            // Find recipes that use at least one ingredient from inventory and no allergy ingredients
            var candidateRecipes = await _context.Recipes
                .Include(r => r.RecipeIngredients)
                .Where(r => r.RecipeIngredients.Any(ri => ingredientIds.Contains(ri.IngredientId))
                            && !r.RecipeIngredients.Any(ri => allergyIds.Contains(ri.IngredientId)))
                .ToListAsync();

            if (!candidateRecipes.Any())
                return null;

            var userMenu = new Menu
            {
                UserId = userId,
                Name = "Автоматическое меню на неделю (из продуктов)",
                CreatedAt = DateTime.UtcNow
            };

            var today = DateTime.Today;
            var mealTypes = new[] { "Завтрак", "Обед", "Ужин" };

            for (int day = 0; day < 7; day++)
            {
                var date = today.AddDays(day);
                foreach (var mealType in mealTypes)
                {
                    var recipe = candidateRecipes[_rnd.Next(candidateRecipes.Count)];
                    userMenu.Items.Add(new MenuItem
                    {
                        RecipeId = recipe.Id,
                        Date = date,
                        MealType = mealType
                    });
                }
            }

            await _context.Menus.AddAsync(userMenu);
            await _context.SaveChangesAsync();

            return userMenu.Id;


        }

        




    }
}
