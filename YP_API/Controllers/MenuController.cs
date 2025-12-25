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
        {
            try
            {
                _logger.LogInformation($"Getting current menu for user ID: {userId}");

                var menu = await _menuService.GetCurrentMenuAsync(userId);

                if (menu == null)
                    return Ok(new
                    {
                        success = false,
                        message = "Текущее меню не существует",
                        data = (object)null
                    });

                return Ok(new
                {
                    success = true,
                    message = "Меню получено",
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
            [Display(Name = "Максимальные калории в день")]
            decimal? targetCaloriesPerDay = null,

            [FromForm]
            [Display(Name = "Теги кухни")]
            List<string> cuisineTags = null,

            [FromForm]
            [Display(Name = "Типы приёмов пищи")]
            List<string> mealTypes = null,

            [FromForm]
            [Display(Name = "Использовать инвентарь")]
            bool useInventory = false)
        {
            try
            {
                _logger.LogInformation($"Generating menu for user {userId}, days: {days}, calories: {targetCaloriesPerDay}");

                var currentMenu = await _menuService.GetCurrentMenuAsync(userId);
                if (currentMenu != null)
                {
                    _logger.LogInformation($"Deleting old menu ID: {currentMenu.Id} before generating new one");
                    await _menuService.DeleteMenuAsync(currentMenu.Id);
                }

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        error = "Пользователь не существует",
                        message = "Не удалось найти указанного пользователя"
                    });
                }

                var availableRecipes = await _recipeRepository.GetRecipesForMenuAsync(
                    user.Allergies ?? new List<string>(),
                    cuisineTags ?? new List<string>(),
                    targetCaloriesPerDay);

                _logger.LogInformation($"Found {availableRecipes.Count()} available recipes for user {userId}");

                if (!availableRecipes.Any())
                {
                    _logger.LogInformation($"No recipes found with filters. Trying without filters...");

                    availableRecipes = await _recipeRepository.GetRecipesForMenuAsync(
                        new List<string>(),
                        new List<string>(),
                        null);

                    _logger.LogInformation($"Now found {availableRecipes.Count()} recipes without filters");

                    if (!availableRecipes.Any())
                    {
                        return BadRequest(new
                        {
                            success = false,
                            error = "В базе данных нет рецептов",
                            message = "В системе не найдено ни одного рецепта. Добавьте рецепты в базу данных."
                        });
                    }
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

        [HttpDelete("clear/{userId}")]
        public async Task<ActionResult> ClearCurrentMenu(int userId)
        {
            try
            {
                _logger.LogInformation($"Clearing current menu for user ID: {userId}");

                var currentMenu = await _menuService.GetCurrentMenuAsync(userId);

                if (currentMenu == null)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "Текущее меню не найдено",
                        data = (object)null
                    });
                }

                // Удаляем меню через сервис
                var result = await _menuService.DeleteMenuAsync(currentMenu.Id);

                if (result)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Текущее меню успешно удалено",
                        data = new
                        {
                            DeletedMenuId = currentMenu.Id,
                            DeletedMenuName = currentMenu.Name
                        }
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        error = "Ошибка удаления меню",
                        message = "Не удалось удалить текущее меню"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in ClearCurrentMenu: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    error = "Внутренняя ошибка сервера",
                    message = "Произошла ошибка при удалении меню"
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