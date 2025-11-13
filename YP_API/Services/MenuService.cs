using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YP_API.Data;
using YP_API.Models;
using YP_API.Interfaces;

namespace YP_API.Services
{
    public class MenuService : IMenuService
    {
        private readonly RecipePlannerContext _context;
        private readonly ILogger<MenuService> _logger;

        public MenuService(RecipePlannerContext context, ILogger<MenuService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<MenuDto> GenerateMenuAsync(int userId, GenerateMenuRequest request)
        {
            try
            {
                _logger.LogInformation($"Generating menu for user {userId}: {request.Days} days, {request.TargetCaloriesPerDay} calories");

                // Get user allergies
                var user = await _context.Users
                    .Include(u => u.Allergies)
                    .FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    _logger.LogError($"User {userId} not found");
                    return null;
                }

                var userAllergies = user.Allergies?.ToList() ?? new List<string>();
                _logger.LogInformation($"User allergies: {string.Join(", ", userAllergies)}");

                // Get inventory
                var inventory = await _context.UserInventories
                    .Include(ui => ui.Ingredient)
                    .Where(ui => ui.UserId == userId)
                    .ToListAsync();

                var availableIngredients = inventory.Select(i => i.IngredientId).ToList();
                var useInventory = request.UseInventory && availableIngredients.Any();
                _logger.LogInformation($"Using inventory: {useInventory}, available: {availableIngredients.Count}");

                // Filter recipes by criteria
                var query = _context.Recipes
                    .Include(r => r.RecipeIngredients)
                    .Include(r => r.Allergens)
                    .Include(r => r.Tags)
                    .AsQueryable();

                // Cuisine filtering
                if (request.CuisineTags != null && request.CuisineTags.Any())
                {
                    var cuisines = request.CuisineTags.Select(t => t.ToLower()).ToList();
                    query = query.Where(r => cuisines.Contains(r.CuisineType?.ToLower()) ||
                                           r.Tags.Any(tag => cuisines.Contains(tag.ToLower())));
                    _logger.LogInformation($"Filtering by cuisines: {string.Join(", ", cuisines)}");
                }

                // Exclude allergens
                if (userAllergies.Any())
                {
                    query = query.Where(r => !r.Allergens.Any(a => userAllergies.Contains(a.ToLower())));
                    _logger.LogInformation($"Excluding allergens: {string.Join(", ", userAllergies)}");
                }

                // Use inventory if requested
                if (useInventory)
                {
                    query = query.Where(r => r.RecipeIngredients.All(ri => availableIngredients.Contains(ri.IngredientId)));
                    _logger.LogInformation("Filtering recipes that can be made from inventory");
                }

                // Time filter
                if (request.MaxPrepTime.HasValue)
                {
                    query = query.Where(r => r.PrepTime + r.CookTime <= request.MaxPrepTime);
                }

                // Get candidate recipes
                var candidateRecipes = await query
                    .Where(r => r.Calories > 0 && r.Servings > 0)
                    .OrderBy(r => r.Difficulty)
                    .ThenBy(r => r.PrepTime + r.CookTime)
                    .Take(50)  // Limit to reasonable number
                    .ToListAsync();

                _logger.LogInformation($"Found {candidateRecipes.Count} candidate recipes");

                if (!candidateRecipes.Any())
                {
                    _logger.LogWarning("No recipes found matching criteria");
                    return null;
                }

                // Generate menu for each day
                var menuMeals = new List<MenuMealDto>();
                var random = new Random();
                var usedRecipes = new HashSet<int>();
                var dailyCalories = 0m;

                for (int day = 0; day < request.Days; day++)
                {
                    var dayCalories = 0m;
                    var dayMeals = new List<string> { "breakfast", "lunch", "dinner" };
                    random.Shuffle(dayMeals);  // Randomize meal order

                    foreach (var mealType in dayMeals)
                    {
                        // Filter recipes by meal type (if tags match)
                        var mealRecipes = candidateRecipes
                            .Where(r => !usedRecipes.Contains(r.Id) &&
                                      (r.Tags.Contains(mealType) || 
                                       (mealType == "breakfast" && r.CookTime <= 30) ||
                                       (mealType == "lunch" && r.Servings >= 2) ||
                                       (mealType == "dinner" && r.Calories >= 500)))
                            .ToList();

                        if (!mealRecipes.Any())
                        {
                            // Fallback to any unused recipe
                            mealRecipes = candidateRecipes
                                .Where(r => !usedRecipes.Contains(r.Id))
                                .OrderBy(r => Math.Abs(r.Calories / r.Servings - (request.TargetCaloriesPerDay / 3)))
                                .Take(5)
                                .ToList();
                        }

                        if (mealRecipes.Any())
                        {
                            var selectedRecipe = mealRecipes[random.Next(mealRecipes.Count)];
                            var mealCalories = selectedRecipe.Calories * 1f / selectedRecipe.Servings;

                            menuMeals.Add(new MenuMealDto
                            {
                                RecipeId = selectedRecipe.Id,
                                MealType = mealType,
                                MealDate = DateTime.Today.AddDays(day),
                                Calories = mealCalories
                            });

                            usedRecipes.Add(selectedRecipe.Id);
                            dayCalories += mealCalories;
                            _logger.LogInformation($"Day {day + 1}, {mealType}: {selectedRecipe.Title} ({mealCalories:F0} cal)");
                        }
                    }

                    dailyCalories += dayCalories;
                }

                // Create weekly menu
                var weeklyMenu = new WeeklyMenu
                {
                    UserId = userId,
                    Name = $"Автоматическое меню с {DateTime.Today:dd.MM.yyyy}",
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddDays(request.Days - 1),
                    TotalCalories = dailyCalories,
                    MenuMeals = menuMeals.Select(m => new MenuMeal
                    {
                        RecipeId = m.RecipeId,
                        MealDate = m.MealDate,
                        MealType = GetMealTypeNumber(m.MealType)
                    }).ToList()
                };

                _context.WeeklyMenus.Add(weeklyMenu);
                await _context.SaveChangesAsync();

                // Generate shopping list
                var shoppingList = await GenerateShoppingList(weeklyMenu.Id);
                weeklyMenu.ShoppingList = shoppingList;

                var result = new MenuDto
                {
                    Id = weeklyMenu.Id,
                    Name = weeklyMenu.Name,
                    StartDate = weeklyMenu.StartDate,
                    EndDate = weeklyMenu.EndDate,
                    TotalCalories = weeklyMenu.TotalCalories,
                    MenuMeals = menuMeals,
                    ShoppingList = shoppingList
                };

                _logger.LogInformation($"Menu generated successfully: {result.Id}, {dailyCalories:F0} total calories");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating menu for user {userId}");
                return null;
            }
        }

        private async Task<ShoppingListDto> GenerateShoppingList(int menuId)
        {
            var menuMeals = await _context.MenuMeals
                .Include(mm => mm.Recipe)
                .ThenInclude(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient)
                .Where(mm => mm.WeeklyMenuId == menuId)
                .ToListAsync();

            var ingredientsNeeded = new Dictionary<int, (decimal Quantity, string Unit)>();

            foreach (var meal in menuMeals)
            {
                foreach (var ri in meal.Recipe.RecipeIngredients)
                {
                    var key = ri.IngredientId;
                    if (ingredientsNeeded.ContainsKey(key))
                    {
                        var current = ingredientsNeeded[key];
                        ingredientsNeeded[key] = (current.Quantity + ri.Quantity, current.Unit);
                    }
                    else
                    {
                        ingredientsNeeded[key] = (ri.Quantity, ri.Unit);
                    }
                }
            }

            var shoppingList = new ShoppingList
            {
                MenuId = menuId,
                UserId = 1,  // Default user
                Name = $"Список покупок для меню #{menuId}",
                IsCompleted = false
            };

            _context.ShoppingLists.Add(shoppingList);
            await _context.SaveChangesAsync();

            foreach (var kvp in ingredientsNeeded)
            {
                var item = new ShoppingListItem
                {
                    ShoppingListId = shoppingList.Id,
                    IngredientId = kvp.Key,
                    Quantity = kvp.Value.Quantity,
                    Unit = kvp.Value.Unit,
                    IsPurchased = false
                };
                _context.ShoppingListItems.Add(item);
            }

            await _context.SaveChangesAsync();

            var dto = new ShoppingListDto
            {
                Id = shoppingList.Id,
                Name = shoppingList.Name,
                Items = ingredientsNeeded.Select(kvp => new ShoppingItemDto
                {
                    IngredientId = kvp.Key,
                    Quantity = kvp.Value.Quantity,
                    Unit = kvp.Value.Unit,
                    IsPurchased = false
                }).ToList()
            };

            return dto;
        }

        private int GetMealTypeNumber(string mealType)
        {
            return mealType.ToLower() switch
            {
                "breakfast" => 1,
                "lunch" => 2,
                "dinner" => 3,
                _ => 1
            };
        }
    }

    public class MenuDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalCalories { get; set; }
        public List<MenuMealDto> MenuMeals { get; set; } = new();
        public ShoppingListDto? ShoppingList { get; set; }
    }

    public class MenuMealDto
    {
        public int RecipeId { get; set; }
        public string MealType { get; set; }
        public DateTime MealDate { get; set; }
        public decimal Calories { get; set; }
    }

    public class ShoppingListDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<ShoppingItemDto> Items { get; set; } = new();
    }

    public class ShoppingItemDto
    {
        public int IngredientId { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
        public bool IsPurchased { get; set; }
    }
}