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
        public async Task<ActionResult> SearchIngredients([FromQuery(Name = "name")] string? searchName)  // Добавил Name = "name" и ?
        {
            try
            {
                IQueryable<Ingredient> query = _context.Ingredients;

                if (!string.IsNullOrWhiteSpace(searchName))
                {
                    query = query.Where(i => i.Name.ToLower().Contains(searchName.ToLower()));
                }

                var ingredients = await query
                    .Take(50)
                    .ToListAsync();

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