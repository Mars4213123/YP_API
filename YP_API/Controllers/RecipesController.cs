using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using YP_API.Helpers;
using YP_API.Interfaces;
using YP_API.Models;

namespace YP_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecipesController : ControllerBase
    {
        private readonly IRecipeRepository _recipeRepository;
        private readonly ILogger<RecipesController> _logger;

        public RecipesController(IRecipeRepository recipeRepository, ILogger<RecipesController> logger)
        {
            _recipeRepository = recipeRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult> GetRecipes([FromQuery] RecipeSearchParams searchParams)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ModelState.Remove("Difficulty");

                    if (ModelState.ErrorCount > 0)
                    {
                        var errors = ModelState
                            .Where(x => x.Key != "Difficulty" && x.Value.Errors.Count > 0)
                            .ToDictionary(
                                x => x.Key,
                                x => x.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                            );

                        if (errors.Count > 0)
                        {
                            return BadRequest(new
                            {
                                success = false,
                                error = "Ошибки валидации",
                                errors = errors
                            });
                        }
                    }
                }

                var recipes = await _recipeRepository.GetRecipesAsync(searchParams);

                Response.Headers.Add("X-Pagination", System.Text.Json.JsonSerializer.Serialize(new
                {
                    recipes.CurrentPage,
                    recipes.PageSize,
                    recipes.TotalCount,
                    recipes.TotalPages
                }));

                return Ok(new
                {
                    success = true,
                    message = "Рецепты получены успешно",
                    data = recipes.Select(r => new {
                        Id = r.Id,
                        Title = r.Title,
                        Description = r.Description,
                        PrepTime = r.PrepTime,
                        CookTime = r.CookTime,
                        Calories = r.Calories,
                        ImageUrl = r.ImageUrl,
                        Difficulty = r.Difficulty
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetRecipes: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    error = "Внутренняя ошибка сервера",
                    message = "Произошла ошибка при получении рецептов"
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetRecipe(int id)
        {
            try
            {
                var recipe = await _recipeRepository.GetRecipeWithDetailsAsync(id);

                if (recipe == null)
                    return NotFound(new
                    {
                        success = false,
                        error = "Рецепт не найден",
                        message = $"Рецепт с ID {id} не существует"
                    });

                return Ok(new
                {
                    success = true,
                    message = "Рецепт получен успешно",
                    data = new
                    {
                        Id = recipe.Id,
                        Title = recipe.Title,
                        Description = recipe.Description,
                        Instructions = recipe.Instructions,
                        PrepTime = recipe.PrepTime,
                        CookTime = recipe.CookTime,
                        Servings = recipe.Servings,
                        Calories = recipe.Calories,
                        ImageUrl = recipe.ImageUrl,
                        Difficulty = recipe.Difficulty,
                        CuisineType = recipe.CuisineType,
                        Ingredients = recipe.RecipeIngredients?.Select(ri => new {
                            Name = ri.Ingredient?.Name,
                            Quantity = ri.Quantity,
                            Unit = ri.Unit,
                            Category = ri.Ingredient?.Category
                        })
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetRecipe: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    error = "Внутренняя ошибка сервера",
                    message = "Произошла ошибка при получении рецепта"
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult> CreateRecipe(
            [FromForm]
            [Required(ErrorMessage = "Название рецепта обязательно")]
            [Display(Name = "Название рецепта")]
            string title,

            [FromForm]
            [Display(Name = "Описание рецепта")]
            string description,

            [FromForm]
            [Required(ErrorMessage = "Инструкции обязательны")]
            [Display(Name = "Инструкции приготовления")]
            string instructions,

            [FromForm]
            [Range(0, 1000, ErrorMessage = "Время подготовки должно быть от 0 до 1000 минут")]
            [Display(Name = "Время подготовки (минуты)")]
            int prepTime,

            [FromForm]
            [Range(0, 1000, ErrorMessage = "Время готовки должно быть от 0 до 1000 минут")]
            [Display(Name = "Время готовки (минуты)")]
            int cookTime,

            [FromForm]
            [Range(1, 100, ErrorMessage = "Количество порций должно быть от 1 до 100")]
            [Display(Name = "Количество порций")]
            int servings,

            [FromForm]
            [Range(0, 10000, ErrorMessage = "Калории должны быть от 0 до 10000")]
            [Display(Name = "Калории")]
            decimal calories,

            [FromForm]
            [Url(ErrorMessage = "Неверный URL изображения")]
            [Display(Name = "URL изображения")]
            string imageUrl = "",

            [FromForm]
            [Display(Name = "Тип кухни")]
            string cuisineType = "",

            [FromForm]
            [Display(Name = "Сложность приготовления")]
            string difficulty = "")
        {
            try
            {
                var recipe = new Recipe
                {
                    Title = title,
                    Description = description,
                    Instructions = instructions,
                    PrepTime = prepTime,
                    CookTime = cookTime,
                    Servings = servings,
                    Calories = calories,
                    ImageUrl = imageUrl,
                    CuisineType = cuisineType,
                    Difficulty = difficulty,
                    CreatedAt = DateTime.UtcNow
                };

                await _recipeRepository.AddAsync(recipe);

                if (await _recipeRepository.SaveAllAsync())
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Рецепт успешно создан",
                        data = new
                        {
                            Id = recipe.Id,
                            Title = recipe.Title
                        }
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    error = "Ошибка создания рецепта",
                    message = "Не удалось сохранить рецепт в базе данных"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in CreateRecipe: {ex.Message}");
                return BadRequest(new
                {
                    success = false,
                    error = "Ошибка создания рецепта",
                    message = ex.Message
                });
            }
        }

        [HttpPost("{id}/favorite/{userId}")]
        public async Task<ActionResult> ToggleFavorite(int id, int userId)
        {
            try
            {
                _logger.LogInformation($"Toggling favorite for user {userId}, recipe {id}");

                var success = await _recipeRepository.ToggleFavoriteAsync(userId, id);

                if (success)
                {
                    var isFavorite = await _recipeRepository.IsRecipeFavoriteAsync(userId, id);

                    return Ok(new
                    {
                        success = true,
                        message = isFavorite ? "Рецепт добавлен в избранное" : "Рецепт удален из избранного",
                        data = new
                        {
                            RecipeId = id,
                            IsFavorite = isFavorite
                        }
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    error = "Ошибка изменения избранного",
                    message = "Не удалось изменить статус избранного"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in ToggleFavorite: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");

                return BadRequest(new
                {
                    success = false,
                    error = "Ошибка изменения избранного",
                    message = ex.Message
                });
            }
        }

        [HttpGet("favorites/{userId}")]
        public async Task<ActionResult> GetFavorites(int userId)
        {
            try
            {
                var favorites = await _recipeRepository.GetUserFavoritesAsync(userId);

                return Ok(new
                {
                    success = true,
                    message = "Избранные рецепты получены",
                    data = favorites.Select(r => new {
                        Id = r.Id,
                        Title = r.Title,
                        Description = r.Description,
                        Calories = r.Calories,
                        ImageUrl = r.ImageUrl,
                        PrepTime = r.PrepTime,
                        CookTime = r.CookTime
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetFavorites: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    error = "Внутренняя ошибка сервера",
                    message = "Произошла ошибка при получении избранных рецептов"
                });
            }
        }
    }

    public class CreateRecipeIngredientRequest
    {
        public int IngredientId { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
    }
}