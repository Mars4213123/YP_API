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
            if (string.IsNullOrWhiteSpace(input.Prompt))
            {
                return BadRequest("Запрос не может быть пустым.");
            }

            try
            {
                var messages = new List<Request.Message>
                {
                    new Request.Message
                    {
                        role = "user",
                        content = input.Prompt
                    }
                };

                var token = await GigaChatHelper.GetToken(GigaChatHelper.ClientId, GigaChatHelper.AuthorizationKey);
                var response = await GigaChatHelper.GetAnswer(token, messages);

                var generatedMenu = await GigaChatHelper.GenerateAndParseMenuAsync(token, input.Ingredients, 1);

                var menuEntity = new Menu
                {
                    UserId = userId,
                    Name = generatedMenu.MenuName,
                    CreatedAt = DateTime.UtcNow,
                    Items = new List<MenuItem>()
                };

                DateTime startDate = DateTime.Today;

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

                        var ingredientEntity = await _context.Ingredients
                            .FirstOrDefaultAsync(i => i.Name == ingName);

                        if (ingredientEntity == null)
                        {
                            ingredientEntity = new Ingredient
                            {
                                Name = ingName,
                                Unit = ingDto.Unit,
                                Category = "Сгенерировано", 
                                Allergens = ""
                            };
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
                    Message = "Меню успешно сгенерировано на основе тестовых продуктов",
                    MenuId = menuEntity.Id,
                    InputIngredients = input.Ingredients.Select(i => i.Name),
                    Result = generatedMenu
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
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