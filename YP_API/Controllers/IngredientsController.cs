using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YP_API.Data;
using YP_API.Models;

namespace YP_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IngredientsController : ControllerBase
    {
        // Используем единое имя поля для контекста базы данных
        private readonly RecipePlannerContext _context;

        public IngredientsController(RecipePlannerContext context)
        {
            _context = context;
        }

        // 1. Метод поиска ингредиентов (совмещенный с созданием, как в твоем примере)
        [HttpGet("search")]
        public async Task<ActionResult> SearchIngredientsFromQuery([FromQuery] string? name, [FromQuery] string? searchName)
        {
            // Берем либо searchName, либо name
            string queryText = searchName ?? name;

            try
            {
                IQueryable<Ingredient> query = _context.Ingredients;

                if (!string.IsNullOrWhiteSpace(queryText))
                {
                    var lower = queryText.ToLower();
                    query = query.Where(i => i.Name.ToLower().Contains(lower));
                }

                var ingredients = await query
                    .Take(50)
                    .ToListAsync();

                // ЛОГИКА АВТОСОЗДАНИЯ:
                // Если ничего не нашли, запрос был по конкретному имени (name), и оно не пустое -> создаем
                if (!ingredients.Any() && !string.IsNullOrWhiteSpace(name))
                {
                    // Проверим еще раз точное совпадение, чтобы не плодить дубли
                    var existing = await _context.Ingredients
                        .FirstOrDefaultAsync(i => i.Name.ToLower() == name.Trim().ToLower());

                    if (existing == null)
                    {
                        var newIngredient = new Ingredient
                        {
                            Name = name.Trim(),
                            Category = "Прочее",
                            Unit = "шт",          // Значение по умолчанию
                            StandardUnit = "шт",  // Если есть такое поле в модели
                            Allergens = ""        // Пустая строка (не null), чтобы база не ругалась
                        };

                        _context.Ingredients.Add(newIngredient);
                        await _context.SaveChangesAsync();

                        // Добавляем созданный ингредиент в список результата
                        ingredients.Add(newIngredient);
                    }
                    else
                    {
                        ingredients.Add(existing);
                    }
                }

                return Ok(new
                {
                    success = true,
                    data = ingredients.Select(i => new
                    {
                        Id = i.Id,
                        Name = i.Name,
                        Category = i.Category,
                        Unit = i.Unit
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // 2. Метод явного создания ингредиента (POST запрос)
        [HttpPost("create")]
        public async Task<IActionResult> CreateIngredient([FromBody] IngredientDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Name))
                    return BadRequest(new { success = false, error = "Имя не может быть пустым" });

                // Проверяем, может он уже есть
                var existing = await _context.Ingredients
                    .FirstOrDefaultAsync(i => i.Name.ToLower() == dto.Name.Trim().ToLower());

                if (existing != null)
                    return Ok(new { success = true, data = new { existing.Id, existing.Name } });

                // Создаем новый
                var newIngredient = new Ingredient
                {
                    Name = dto.Name.Trim(),
                    Unit = "шт",
                    Category = "Другое",
                    StandardUnit = "шт",
                    Allergens = "" // Важно! Пустая строка
                };

                _context.Ingredients.Add(newIngredient);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, data = new { newIngredient.Id, newIngredient.Name } });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        // 3. Получение всех категорий
        [HttpGet("categories")]
        public async Task<ActionResult> GetCategories()
        {
            try
            {
                var categories = await _context.Ingredients
                    .Where(i => !string.IsNullOrEmpty(i.Category))
                    .Select(i => i.Category)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = categories
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Простая модель DTO для приема данных в CreateIngredient
    public class IngredientDto
    {
        public string Name { get; set; }
    }
}
