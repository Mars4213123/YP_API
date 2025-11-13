using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using YP_API.Interfaces;
using YP_API.Models;
using YP_API.Services;

namespace YP_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MenuController : ControllerBase
    {
        private readonly IMenuService _menuService;
        private readonly IUserRepository _userRepository;
        private readonly IRecipeRepository _recipeRepository;
        private readonly ILogger<MenuController> _logger;

        public MenuController(IMenuService menuService, IUserRepository userRepository, IRecipeRepository recipeRepository, ILogger<MenuController> logger)
        {
            _menuService = menuService;
            _userRepository = userRepository;
            _recipeRepository = recipeRepository;
            _logger = logger;
        }

        [HttpGet("current/{userId}")]
        public async Task<ActionResult> GetCurrentMenu(int userId)
        {  // asd 
            try
            {
                _logger.LogInformation($"Getting current menu for user ID: {userId}");

                var menu = await _menuService.GetCurrentMenuAsync(userId);

                if (menu == null)
                    return Ok(new
                    {
                        success = false,
                        message = "Текущее меню не найдено",
                        data = (object)null
                    });

                return Ok(new
                {
                    success = true,
                    message = "Меню найдено",
                    data = new
                    {
                        Id = menu.Id,
                        Name = menu.Name,
                        StartDate = menu.StartDate,
                        EndDate = menu.EndDate,
                        TotalCalories = menu.TotalCalories,
                        Days = menu.MenuMeals?.GroupBy(m => m.MealDate).Select(g => new {
                            Date = g.Key,
                            Meals = g.Select(m => new {
                                Id = m.Id,
                                RecipeId = m.RecipeId,
                                RecipeTitle = m.Recipe?.Title,
                                MealType = m.MealType,
                                Calories = m.Recipe?.Calories,
                                PrepTime = (m.Recipe?.PrepTime ?? 0) + (m.Recipe?.CookTime ?? 0),
                                ImageUrl = m.Recipe?.ImageUrl
                            })
                        })
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetCurrentMenu: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    error = "Внутренняя ошибка сервера",
                    message = "Произошла ошибка при получении меню"
                });
            }
        }

        [HttpPost("generate/{userId}")]
        public async Task<ActionResult> GenerateMenu(
            int userId,
            [FromForm]
            [Range(1, 30, ErrorMessage = "Количество дней должно быть от 1 до 30")]
            [Display(Name = "Количество дней")]
            int days = 7,

            [FromForm]
            [Range(0, 10000, ErrorMessage = "Калории должны быть от 0 до 10000")]
            [Display(Name = "Желаемые калории в день")]
            decimal? targetCaloriesPerDay = null,

            [FromForm]
            [Display(Name = "Тип кухни")]
            List<string> cuisineTags = null,

            [FromForm]
            [Display(Name = "Типы приемов пищи")]
            List<string> mealTypes = null,

            [FromForm]
            [Display(Name = "Использовать инвентарь")]
            bool useInventory = false)
        {
            try
            {
                _logger.LogInformation($"Generating menu for user {userId}, days: {days}, calories: {targetCaloriesPerDay}");

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        error = "Пользователь не найден",
                        message = "Не удалось найти данного пользователя"
                    });
                }

                var availableRecipes = await _recipeRepository.GetRecipesForMenuAsync(
                    user.Allergies ?? new List<string>(),
                    cuisineTags ?? new List<string>(),
                    targetCaloriesPerDay);

                if (!availableRecipes.Any())
                {
                    _logger.LogWarning($"No recipes available for user {userId} with filters: allergies={user.Allergies?.Count}, cuisineTags={cuisineTags?.Count}, maxCalories={targetCaloriesPerDay}");

                    return BadRequest(new
                    {
                        success = false,
                        error = "Нет доступных рецептов",
                        message = "Не найдено рецептов по заданным фильтрам. Попробуйте изменить фильтры.",
                        suggestions = new
                        {
                            tryWithoutAllergies = user.Allergies?.Any() == true,
                            tryWithoutCuisineTags = cuisineTags?.Any() == true,
                            tryHigherCalories = targetCaloriesPerDay.HasValue
                        }
                    });
                }

                var request = new GenerateMenuRequest
                {
                    Days = days,
                    TargetCaloriesPerDay = targetCaloriesPerDay,
                    CuisineTags = cuisineTags ?? new List<string>(),
                    MealTypes = mealTypes ?? new List<string> { "breakfast", "lunch", "dinner" },
                    UseInventory = useInventory
                };

                var menu = await _menuService.GenerateWeeklyMenuAsync(userId, request, user.Allergies);

                _logger.LogInformation($"Menu generated successfully: {menu.Id} with {menu.MenuMeals?.Count} meals");

                return Ok(new
                {
                    success = true,
                    message = "Меню успешно сгенерировано",
                    data = new
                    {
                        Id = menu.Id,
                        Name = menu.Name,
                        StartDate = menu.StartDate,
                        EndDate = menu.EndDate,
                        TotalCalories = menu.TotalCalories,
                        MealCount = menu.MenuMeals?.Count ?? 0
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GenerateMenu: {ex.Message}");
                return BadRequest(new
                {
                    success = false,
                    error = "Ошибка генерации меню",
                    message = ex.Message
                });
            }
        }

        [HttpGet("history/{userId}")]
        public async Task<ActionResult> GetMenuHistory(int userId)
        {
            try
            {
                var menus = await _menuService.GetUserMenuHistoryAsync(userId);

                if (!menus.Any())
                {
                    return Ok(new
                    {
                        success = true,
                        message = "История меню пуста",
                        data = new List<object>()
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "История меню получена",
                    data = menus.Select(m => new {
                        Id = m.Id,
                        Name = m.Name,
                        StartDate = m.StartDate,
                        EndDate = m.EndDate,
                        TotalCalories = m.TotalCalories,
                        CreatedAt = m.CreatedAt
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetMenuHistory: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    error = "Внутренняя ошибка сервера",
                    message = "Произошла ошибка при получении истории"
                });
            }
        }

        [HttpPost("{menuId}/regenerate-day")]
        public async Task<ActionResult> RegenerateDay(
            int menuId,
            [FromForm]
            [Required(ErrorMessage = "Дата обязательна")]
            [Display(Name = "Дата для перегенерации")]
            DateTime date)
        {
            try
            {
                var menu = await _menuService.RegenerateDayAsync(menuId, date, null);

                return Ok(new
                {
                    success = true,
                    message = "День успешно перегенерирован",
                    data = new
                    {
                        MenuId = menu.Id,
                        Date = date
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in RegenerateDay: {ex.Message}");
                return BadRequest(new
                {
                    success = false,
                    error = "Ошибка перегенерации",
                    message = ex.Message
                });
            }
        }
    }

    public class GenerateMenuRequest
    {
        public int Days { get; set; } = 7;
        public decimal? TargetCaloriesPerDay { get; set; }
        public List<string> CuisineTags { get; set; } = new List<string>();
        public List<string> MealTypes { get; set; } = new List<string> { "breakfast", "lunch", "dinner" };
        public bool UseInventory { get; set; } = false;
    }
}