using YP_API.Controllers;
using YP_API.Interfaces;
using YP_API.Models;
using YP_API.Repositories;

namespace YP_API.Services
{
    public class MenuService : IMenuService
    {
        private readonly IMenuRepository _menuRepository;
        private readonly IRecipeRepository _recipeRepository;
        private readonly ILogger<MenuService> _logger;

        public MenuService(IMenuRepository menuRepository, IRecipeRepository recipeRepository, ILogger<MenuService> logger)
        {
            _menuRepository = menuRepository;
            _recipeRepository = recipeRepository;
            _logger = logger;
        }

        public async Task<WeeklyMenu> GenerateWeeklyMenuAsync(int userId, Controllers.GenerateMenuRequest request, List<string> userAllergies)
        {
            _logger.LogInformation($"Generating menu for user {userId}, days: {request.Days}");

            var allRecipes = (await _recipeRepository.GetRecipesForMenuAsync(
                userAllergies ?? new List<string>(),
                request.CuisineTags ?? new List<string>(),
                request.TargetCaloriesPerDay)).ToList();

            var availableRecipes = allRecipes.AsEnumerable();

            if (userAllergies != null && userAllergies.Any())
            {
                availableRecipes = availableRecipes
                    .Where(r => !r.Allergens.Any(a => userAllergies.Contains(a)));
            }

            if (request.CuisineTags != null && request.CuisineTags.Any())
            {
                availableRecipes = availableRecipes
                    .Where(r => request.CuisineTags.Contains(r.CuisineType));
            }

            if (request.TargetCaloriesPerDay.HasValue)
            {
                availableRecipes = availableRecipes
                    .Where(r => r.Calories <= request.TargetCaloriesPerDay.Value);
            }

            var finalRecipes = availableRecipes.ToList();

            _logger.LogInformation($"Found {finalRecipes.Count} available recipes");

            if (!finalRecipes.Any())
            {
                throw new Exception("В базе данных нет доступных рецептов для генерации меню");
            }

            var menu = new WeeklyMenu
            {
                UserId = userId,
                Name = $"Меню на {request.Days} дней",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(request.Days - 1),
                CreatedAt = DateTime.UtcNow
            };

            var random = new Random();
            var totalCalories = 0m;

            var breakfastRecipes = finalRecipes
                .Where(r => r.Tags.Contains("breakfast") || IsSuitableForMealType(r, "breakfast"))
                .ToList();

            var lunchRecipes = finalRecipes
                .Where(r => r.Tags.Contains("lunch") || IsSuitableForMealType(r, "lunch"))
                .ToList();

            var dinnerRecipes = finalRecipes
                .Where(r => r.Tags.Contains("dinner") || IsSuitableForMealType(r, "dinner"))
                .ToList();

            if (!breakfastRecipes.Any()) breakfastRecipes = finalRecipes.ToList();
            if (!lunchRecipes.Any()) lunchRecipes = finalRecipes.ToList();
            if (!dinnerRecipes.Any()) dinnerRecipes = finalRecipes.ToList();

            var shuffledBreakfast = breakfastRecipes.OrderBy(x => random.Next()).ToList();
            var shuffledLunch = lunchRecipes.OrderBy(x => random.Next()).ToList();
            var shuffledDinner = dinnerRecipes.OrderBy(x => random.Next()).ToList();

            for (int day = 0; day < request.Days; day++)
            {
                var date = DateTime.Today.AddDays(day);

                if (shuffledBreakfast.Any())
                {
                    var breakfast = shuffledBreakfast[day % shuffledBreakfast.Count];
                    menu.MenuMeals.Add(new MenuMeal
                    {
                        MealDate = date,
                        MealType = MealType.Breakfast,
                        RecipeId = breakfast.Id
                    });
                    totalCalories += breakfast.Calories;
                    _logger.LogInformation($"Added breakfast: {breakfast.Title} ({breakfast.Calories} cal)");
                }

                if (shuffledLunch.Any())
                {
                    var lunch = shuffledLunch[day % shuffledLunch.Count];
                    menu.MenuMeals.Add(new MenuMeal
                    {
                        MealDate = date,
                        MealType = MealType.Lunch,
                        RecipeId = lunch.Id
                    });
                    totalCalories += lunch.Calories;
                    _logger.LogInformation($"Added lunch: {lunch.Title} ({lunch.Calories} cal)");
                }

                if (shuffledDinner.Any())
                {
                    var dinner = shuffledDinner[day % shuffledDinner.Count];
                    menu.MenuMeals.Add(new MenuMeal
                    {
                        MealDate = date,
                        MealType = MealType.Dinner,
                        RecipeId = dinner.Id
                    });
                    totalCalories += dinner.Calories;
                    _logger.LogInformation($"Added dinner: {dinner.Title} ({dinner.Calories} cal)");
                }
            }

            menu.TotalCalories = totalCalories;

            var createdMenu = await _menuRepository.CreateMenuAsync(menu);
            await _menuRepository.SaveAllAsync();

            _logger.LogInformation($"Menu created successfully with ID: {createdMenu.Id}, {menu.MenuMeals.Count} meals, {totalCalories} total calories");

            return await GetMenuWithDetailsAsync(createdMenu.Id);
        }

        public async Task<WeeklyMenu> GetCurrentMenuAsync(int userId)
        {
            return await _menuRepository.GetCurrentMenuAsync(userId);
        }

        public async Task<List<WeeklyMenu>> GetUserMenuHistoryAsync(int userId)
        {
            var menus = await _menuRepository.GetUserMenusAsync(userId);
            return menus.ToList();
        }

        public async Task<WeeklyMenu> RegenerateDayAsync(int menuId, DateTime date, List<string> userAllergies)
        {
            return await _menuRepository.GetMenuWithDetailsAsync(menuId);
        }

        private async Task<WeeklyMenu> GetMenuWithDetailsAsync(int menuId)
        {
            return await _menuRepository.GetMenuWithDetailsAsync(menuId);
        }

        private bool IsSuitableForMealType(Recipe recipe, string mealType)
        {
            if (recipe.Tags == null || !recipe.Tags.Any())
                return false;

            return mealType.ToLower() switch
            {
                "breakfast" => recipe.Tags.Any(t => t.Contains("breakfast") || t.Contains("morning") || t.Contains("завтрак")),
                "lunch" => recipe.Tags.Any(t => t.Contains("lunch") || t.Contains("main") || t.Contains("обед")),
                "dinner" => recipe.Tags.Any(t => t.Contains("dinner") || t.Contains("main") || t.Contains("ужин")),
                "snack" => recipe.Tags.Any(t => t.Contains("snack") || t.Contains("quick") || t.Contains("перекус")),
                _ => false
            };
        }
        public async Task<bool> DeleteMenuAsync(int menuId)
        {
            try
            {
                var menu = await _menuRepository.GetByIdAsync(menuId);
                if (menu == null)
                {
                    _logger.LogWarning($"Menu with ID {menuId} not found for deletion");
                    return false;
                }

                _menuRepository.Delete(menu);
                return await _menuRepository.SaveAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting menu {menuId}: {ex.Message}");
                return false;
            }
        }
    }

    public interface IMenuService
    {
        Task<WeeklyMenu> GenerateWeeklyMenuAsync(int userId, GenerateMenuRequest request, List<string> userAllergies);
        Task<WeeklyMenu> GetCurrentMenuAsync(int userId);
        Task<bool> DeleteMenuAsync(int menuId);
    }
}