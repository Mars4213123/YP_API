using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YP_API.Data;
using YP_API.Helpers;
using YP_API.Models;
using YP_API.Models.AIAPI;

namespace YP_API.Controllers
{

    public class UserQueryDto
    {
        public string Prompt { get; set; }
        public List<Ingredient> Ingredients { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class AiController : ControllerBase
    {
        private readonly RecipePlannerContext _context;

        public AiController(RecipePlannerContext context)
        {
            _context = context;
        }

        [HttpPost("ask/{userId}")]
        public async Task<IActionResult> AskGigaChat(int userId, [FromBody] UserQueryDto input)
        {
            if (string.IsNullOrWhiteSpace(input.Prompt) && (input.Ingredients == null || !input.Ingredients.Any()))
            {
                return BadRequest("Запрос не может быть пустым.");
            }

            try
            {
                // 1. Получаем токен и ответ от AI (без изменений)
                var token = await GigaChatHelper.GetToken(GigaChatHelper.ClientId, GigaChatHelper.AuthorizationKey);
                // ... логика получения промпта ...
                var generatedMenu = await GigaChatHelper.GenerateAndParseMenuAsync(token, input.Ingredients, 1);

                if (generatedMenu == null) return StatusCode(502, "Ошибка генерации меню.");

                var menuEntity = new Menu
                {
                    UserId = userId,
                    Name = generatedMenu.MenuName,
                    CreatedAt = DateTime.UtcNow,
                    Items = new List<MenuItem>()
                };

                DateTime startDate = DateTime.Today;

                // === ИСПРАВЛЕНИЕ НАЧИНАЕТСЯ ЗДЕСЬ ===

                // Создаем словарь для отслеживания ингредиентов в рамках ЭТОГО запроса.
                // Ключ: Название в нижнем регистре, Значение: Сущность ингредиента
                var localIngredientCache = new Dictionary<string, Ingredient>();

                foreach (var itemDto in generatedMenu.Items)
                {
                    var recipeEntity = new Recipe
                    {
                        Title = itemDto.Recipe.Title,
                        Description = itemDto.Recipe.Description,
                        Instructions = string.Join("\n", itemDto.Recipe.Instructions),
                        Calories = itemDto.Recipe.Calories,
                        PrepTime = itemDto.Recipe.PrepTime,
                        CookTime = itemDto.Recipe.CookTime,
                        ImageUrl = "",
                        RecipeIngredients = new List<RecipeIngredient>()
                    };

                    foreach (var ingDto in itemDto.Recipe.Ingredients)
                    {
                        var ingName = ingDto.Name.Trim();
                        var ingKey = ingName.ToLower(); // Ключ для поиска

                        Ingredient ingredientEntity;

                        // ШАГ 1: Проверяем, обрабатывали ли мы этот ингредиент в текущем цикле
                        if (localIngredientCache.ContainsKey(ingKey))
                        {
                            ingredientEntity = localIngredientCache[ingKey];
                        }
                        else
                        {
                            // ШАГ 2: Если нет в кэше, ищем в БД
                            ingredientEntity = await _context.Ingredients
                                .FirstOrDefaultAsync(i => i.Name == ingName);

                            // ШАГ 3: Если нет в БД, создаем новый
                            if (ingredientEntity == null)
                            {
                                ingredientEntity = new Ingredient
                                {
                                    Name = ingName,
                                    Unit = ingDto.Unit,
                                    Category = "Сгенерировано",
                                    Allergens = ""
                                };
                                // Важно: Явно добавляем в контекст, чтобы EF знал о нем
                                _context.Ingredients.Add(ingredientEntity);
                            }

                            // ШАГ 4: Добавляем в локальный кэш, чтобы следующий рецепт использовал ЭТОТ ЖЕ объект
                            localIngredientCache[ingKey] = ingredientEntity;
                        }

                        recipeEntity.RecipeIngredients.Add(new RecipeIngredient
                        {
                            Ingredient = ingredientEntity,
                            Quantity = ingDto.Quantity
                        });
                    }

                    var menuItemEntity = new MenuItem
                    {
                        Date = startDate.AddDays(itemDto.DayNumber - 1),
                        MealType = itemDto.MealType,
                        Recipe = recipeEntity
                    };
                    menuEntity.Items.Add(menuItemEntity);
                }

                _context.Menus.Add(menuEntity);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Меню успешно сгенерировано",
                    MenuId = menuEntity.Id,
                    Result = generatedMenu
                });
            }
            catch (Exception ex)
            {
                // Логируем внутреннее исключение, так как оно содержит детали SQL ошибки
                var innerMessage = ex.InnerException?.Message ?? "";
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message} {innerMessage}");
            }
        }

        [HttpGet("test-generation-real")]
        public async Task<IActionResult> TestGenerationWithMockData()
        {
            try
            {
                var mockIngredients = new List<Ingredient>
                {
                    new Ingredient { Name = "Куриная грудка", Unit = "кг" },
                    new Ingredient { Name = "Рис", Unit = "кг" },
                    new Ingredient { Name = "Помидоры", Unit = "шт" },
                    new Ingredient { Name = "Сметана", Unit = "г" },
                    new Ingredient { Name = "Чеснок", Unit = "зуб" }
                };

                string token = await GigaChatHelper.GetToken(GigaChatHelper.ClientId, GigaChatHelper.AuthorizationKey);

                if (string.IsNullOrEmpty(token))
                {
                    return StatusCode(500, "Ошибка получения токена (Token is null)");
                }

                var generatedMenu = await GigaChatHelper.GenerateAndParseMenuAsync(token, mockIngredients, 1);

                if (generatedMenu == null)
                {
                    return StatusCode(502, "GigaChat вернул пустой ответ или ошибка парсинга.");
                }

                // 4. Возвращаем результат
                return Ok(new
                {
                    Message = "Меню успешно сгенерировано на основе тестовых продуктов",
                    InputIngredients = mockIngredients.Select(i => i.Name),
                    Result = generatedMenu
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренняя ошибка: {ex.Message}");
            }
        }
    }
}