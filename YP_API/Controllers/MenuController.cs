using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YP_API.Data;
using YP_API.Models;

namespace YP_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MenuController : ControllerBase
    {
        private readonly RecipePlannerContext _context;

        public MenuController(RecipePlannerContext context)
        {
            _context = context;
        }

        // ПОЛУЧИТЬ меню для выбора пользователем
        [HttpGet("available")]
        public async Task<ActionResult> GetAvailableMenus()
        {
            try
            {
                var menus = await _context.Menus
                    .Include(m => m.Items)
                        .ThenInclude(i => i.Recipe)
                    .Select(m => new
                    {
                        Id = m.Id,
                        Name = m.Name,
                        CreatedAt = m.CreatedAt,
                        TotalDays = m.Items.Select(i => i.Date.Date).Distinct().Count(),
                        Recipes = m.Items.Select(i => new
                        {
                            Id = i.RecipeId,
                            Title = i.Recipe.Title,
                            Date = i.Date.ToString("yyyy-MM-dd"),
                            MealType = i.MealType
                        })
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = menus
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ВЫБРАТЬ меню пользователем
        [HttpPost("{menuId}/select/{userId}")]
        public async Task<ActionResult> SelectMenu(int menuId, int userId)
        {
            try
            {
                // Проверяем, существует ли меню
                var menu = await _context.Menus
                    .Include(m => m.Items)
                    .FirstOrDefaultAsync(m => m.Id == menuId);

                if (menu == null)
                    return NotFound(new { error = "Меню не найдено" });

                // Проверяем, существует ли пользователь
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return NotFound(new { error = "Пользователь не найден" });

                // Создаем копию меню для пользователя (или можно просто сохранить связь)
                var userMenu = new Menu
                {
                    UserId = userId,
                    Name = $"{menu.Name} (Выбрано)",
                    CreatedAt = DateTime.UtcNow
                };

                // Копируем блюда из выбранного меню
                foreach (var item in menu.Items)
                {
                    userMenu.Items.Add(new MenuItem
                    {
                        RecipeId = item.RecipeId,
                        Date = item.Date,
                        MealType = item.MealType
                    });
                }

                await _context.Menus.AddAsync(userMenu);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Меню успешно выбрано",
                    menuId = userMenu.Id
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        // POST api/menu/generate-week/{userId}
        [HttpPost("generate-week/{userId}")]
        public async Task<ActionResult> GenerateWeekMenu(int userId)
        {
            try
            {
                // Get user's inventory ingredient ids
                var inventoryIngredientIds = await _context.UserInventories
                    .Where(ui => ui.UserId == userId)
                    .Select(ui => ui.IngredientId)
                    .ToListAsync();

                // Get user's allergy ingredient ids
                var allergyIngredientIds = await _context.UserAllergies
                    .Where(a => a.UserId == userId)
                    .Select(a => a.IngredientId)
                    .ToListAsync();

                // Find recipes that:
                // 1. Use at least one ingredient from inventory
                // 2. Don't contain any allergy ingredients
                var candidateRecipes = await _context.Recipes
                    .Include(r => r.RecipeIngredients)
                    .Where(r => inventoryIngredientIds.Any()
                                ? r.RecipeIngredients.Any(ri => inventoryIngredientIds.Contains(ri.IngredientId))
                                  && !r.RecipeIngredients.Any(ri => allergyIngredientIds.Contains(ri.IngredientId))
                                : !r.RecipeIngredients.Any(ri => allergyIngredientIds.Contains(ri.IngredientId)))
                    .ToListAsync();

                if (!candidateRecipes.Any())
                    return BadRequest(new { error = "Нет рецептов, подходящих по ингредиентам и аллергиям" });

                var userMenu = new Menu
                {
                    UserId = userId,
                    Name = "Автоматическое меню на неделю",
                    CreatedAt = DateTime.UtcNow
                };

                var today = DateTime.Today;
                var mealTypes = new[] { "Завтрак", "Обед", "Ужин" };
                var rnd = new Random();
                for (int day = 0; day < 7; day++)
                {
                    var date = today.AddDays(day);
                    foreach (var mealType in mealTypes)
                    {
                        var recipe = candidateRecipes[rnd.Next(candidateRecipes.Count)];
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

                return Ok(new
                {
                    success = true,
                    menuId = userMenu.Id
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("user/{userId}/current")]
        public async Task<ActionResult> GetCurrentMenu(int userId)
        {
            try
            {
                var menu = await _context.Menus
                    .Include(m => m.Items)
                        .ThenInclude(i => i.Recipe)
                    .Where(m => m.UserId == userId)
                    .OrderByDescending(m => m.CreatedAt)
                    .FirstOrDefaultAsync();

                if (menu == null)
                    return Ok(new
                    {
                        success = true,
                        data = (object)null,
                        message = "У пользователя нет выбранного меню"
                    });

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        Id = menu.Id,
                        Name = menu.Name,
                        Items = menu.Items.Select(i => new
                        {
                            RecipeId = i.RecipeId,
                            RecipeTitle = i.Recipe.Title,
                            Date = i.Date.ToString("yyyy-MM-dd"),
                            MealType = i.MealType
                        })
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}