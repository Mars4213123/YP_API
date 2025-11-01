using YP_API.DTOs;
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

        public async Task<WeeklyMenuDto> GenerateWeeklyMenuAsync(int userId, GenerateMenuRequestDto request, List<string> userAllergies)
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

            return await GetMenuDtoAsync(createdMenu.Id);
        }

        public async Task<WeeklyMenuDto> GetCurrentMenuAsync(int userId)
        {
            var menu = await _menuRepository.GetCurrentMenuAsync(userId);
            return menu != null ? await GetMenuDtoAsync(menu.Id) : null;
        }

        public async Task<List<WeeklyMenuDto>> GetUserMenuHistoryAsync(int userId)
        {
            var menus = await _menuRepository.GetUserMenusAsync(userId);
            var menuDtos = new List<WeeklyMenuDto>();

            foreach (var menu in menus)
            {
                menuDtos.Add(await GetMenuDtoAsync(menu.Id));
            }

            return menuDtos;
        }

        public async Task<MenuDayDto> RegenerateDayAsync(int menuId, DateTime date, List<string> userAllergies)
        {
            var menu = await _menuRepository.GetMenuWithDetailsAsync(menuId);
            if (menu == null) return null;

            var dayMeals = menu.MenuMeals.Where(m => m.MealDate.Date == date.Date).ToList();

            return new MenuDayDto
            {
                Date = date,
                Meals = dayMeals.Select(m => new MenuItemDto
                {
                    Id = m.Id,
                    RecipeId = m.Recipe.Id,
                    RecipeTitle = m.Recipe.Title,
                    MealType = m.MealType,
                    Calories = m.Recipe.Calories,
                    PrepTime = m.Recipe.PrepTime + m.Recipe.CookTime,
                    ImageUrl = m.Recipe.ImageUrl ?? string.Empty
                }).ToList()
            };
        }

        private async Task<WeeklyMenuDto> GetMenuDtoAsync(int menuId)
        {
            var menu = await _menuRepository.GetMenuWithDetailsAsync(menuId);
            if (menu == null) return null;

            var menuDto = new WeeklyMenuDto
            {
                Id = menu.Id,
                Name = menu.Name,
                StartDate = menu.StartDate,
                EndDate = menu.EndDate,
                TotalCalories = menu.TotalCalories,
                Days = new List<MenuDayDto>(),
                ShoppingList = new ShoppingListDto()
            };

            // ShoppingList всегда будет пустым, так как мы его не генерируем
            // Но оставляем структуру для будущего использования

            var mealsByDate = menu.MenuMeals.GroupBy(m => m.MealDate.Date);

            foreach (var dayGroup in mealsByDate)
            {
                var dayDto = new MenuDayDto
                {
                    Date = dayGroup.Key,
                    Meals = dayGroup.Select(m => new MenuItemDto
                    {
                        Id = m.Id,
                        RecipeId = m.Recipe.Id,
                        RecipeTitle = m.Recipe.Title,
                        MealType = m.MealType,
                        Calories = m.Recipe.Calories,
                        PrepTime = m.Recipe.PrepTime + m.Recipe.CookTime,
                        ImageUrl = m.Recipe.ImageUrl ?? string.Empty
                    }).ToList()
                };

                menuDto.Days.Add(dayDto);
            }

            return menuDto;
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
        Task<WeeklyMenuDto> GenerateWeeklyMenuAsync(int userId, GenerateMenuRequestDto request, List<string> userAllergies);
        Task<WeeklyMenuDto> GetCurrentMenuAsync(int userId);
        Task<List<WeeklyMenuDto>> GetUserMenuHistoryAsync(int userId);
        Task<MenuDayDto> RegenerateDayAsync(int menuId, DateTime date, List<string> userAllergies);
    }
}

