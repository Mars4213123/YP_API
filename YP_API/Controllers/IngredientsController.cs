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
        private readonly RecipePlannerContext _context;

        public IngredientsController(RecipePlannerContext context)
        {
            _context = context;
        }

       


        [HttpGet("search")]
public async Task<ActionResult> SearchIngredientsFromQuery([FromQuery] string? name, [FromQuery] string? searchName)
{
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

        // Если ничего не нашли и передано name — создаём новый ингредиент
        if (!ingredients.Any() && !string.IsNullOrWhiteSpace(name))
        {
            var newIngredient = new Ingredient
            {
                Name = name.Trim(),
                Category = "Прочее",
                Unit = "",          // или "шт"
                StandardUnit = "",  // если есть такое поле
                Allergens = null
            };

            _context.Ingredients.Add(newIngredient);
            await _context.SaveChangesAsync();

            ingredients.Add(newIngredient);
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


        // Получение всех категорий
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
}