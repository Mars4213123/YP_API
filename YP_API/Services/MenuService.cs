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

            var availableRecipes = (await _recipeRepository.GetRecipesForMenuAsync(
                userAllergies ?? new List<string>(),
                request.CuisineTags ?? new List<string>(),
                request.TargetCaloriesPerDay)).ToList();

            _logger.LogInformation($"Found {availableRecipes.Count} available recipes");

            if (!availableRecipes.Any())
            {
                throw new Exception("No recipes available for menu generation");
            }

            var menu = new WeeklyMenu
            {
                UserId = userId,
                Name = $"Menu Plan {DateTime.Today:yyyy-MM-dd}",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(request.Days - 1),
                CreatedAt = DateTime.UtcNow
            };

            var random = new Random();
            var totalCalories = 0m;

            for (int i = 0; i < request.Days; i++)
            {
                var date = DateTime.Today.AddDays(i);

                foreach (var mealType in request.MealTypes ?? new List<string> { "breakfast", "lunch", "dinner" })
                {
                    var suitableRecipes = availableRecipes
                        .Where(r => r.Tags.Contains(mealType) || IsSuitableForMealType(r, mealType))
                        .ToList();

                    if (suitableRecipes.Any())
                    {
                        var selectedRecipe = suitableRecipes[random.Next(suitableRecipes.Count)];

                        menu.MenuMeals.Add(new MenuMeal
                        {
                            MealDate = date,
                            MealType = Enum.Parse<MealType>(mealType, true),
                            RecipeId = selectedRecipe.Id
                        });

                        totalCalories += selectedRecipe.Calories;
                        availableRecipes.Remove(selectedRecipe);
                    }
                }
            }

            menu.TotalCalories = totalCalories;

            var createdMenu = await _menuRepository.CreateMenuAsync(menu);
            await _menuRepository.SaveAllAsync();

            _logger.LogInformation($"Menu created successfully with ID: {createdMenu.Id}");

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
                "breakfast" => recipe.Tags.Any(t => t.Contains("breakfast") || t.Contains("morning")),
                "lunch" => recipe.Tags.Any(t => t.Contains("lunch") || t.Contains("main")),
                "dinner" => recipe.Tags.Any(t => t.Contains("dinner") || t.Contains("main")),
                "snack" => recipe.Tags.Any(t => t.Contains("snack") || t.Contains("quick")),
                _ => false
            };
        }
    }

    public interface IMenuService
    {
        Task<WeeklyMenu> GenerateWeeklyMenuAsync(int userId, Controllers.GenerateMenuRequest request, List<string> userAllergies);
        Task<WeeklyMenu> GetCurrentMenuAsync(int userId);
        Task<List<WeeklyMenu>> GetUserMenuHistoryAsync(int userId);
        Task<WeeklyMenu> RegenerateDayAsync(int menuId, DateTime date, List<string> userAllergies);
    }
}


