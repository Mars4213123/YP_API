using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YP_API.Data;
using YP_API.Models;

namespace YP_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly RecipePlannerContext _context;

        public InventoryController(RecipePlannerContext context)
        {
            _context = context;
        }

        // ДОБАВИТЬ/ОБНОВИТЬ продукт по IngredientId
        [HttpPost("add/{userId}")]
        public async Task<IActionResult> AddProduct(int userId, [FromForm] string productName, [FromForm] decimal quantity = 1, [FromForm] string unit = "шт")
        {
            try
            {
                var ingredient = await _context.Ingredients
                    .FirstOrDefaultAsync(i => i.Name.ToLower() == productName.ToLower());

                if (ingredient == null)
                {
                    ingredient = new Ingredient { Name = productName };
                    _context.Ingredients.Add(ingredient);
                    await _context.SaveChangesAsync();
                }

                var existing = await _context.UserInventories
                    .FirstOrDefaultAsync(ui => ui.UserId == userId && ui.IngredientId == ingredient.Id);

                if (existing != null)
                {
                    existing.Quantity += quantity;
                }
                else
                {
                    _context.UserInventories.Add(new UserInventory
                    {
                        UserId = userId,
                        IngredientId = ingredient.Id,
                        Quantity = quantity,
                        Unit = unit
                    });
                }

                await _context.SaveChangesAsync();
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }


        public class InventoryItemDto
        {
            public int IngredientId { get; set; }
            public double Quantity { get; set; }
            public string? Unit { get; set; }
        }
    }
}
