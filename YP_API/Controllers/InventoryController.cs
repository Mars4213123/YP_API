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
        [HttpPost("set/{userId}")]
        public async Task<ActionResult> SetInventory(int userId, [FromBody] List<InventoryItemDto> items)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return NotFound(new { error = "Пользователь не найден" });

                // Удаляем старый инвентарь пользователя
                var existing = await _context.UserInventories
                    .Where(ui => ui.UserId == userId)
                    .ToListAsync();

                _context.UserInventories.RemoveRange(existing);

                // Добавляем новые записи (по одной на IngredientId)
                foreach (var it in items)
                {
                    // на всякий случай избегаем дублей в самом списке
                    if (!await _context.Ingredients.AnyAsync(i => i.Id == it.IngredientId))
                        throw new Exception($"Ингредиент {it.IngredientId} не найден в базе");

                    _context.UserInventories.Add(new UserInventory
                    {
                        UserId = userId,
                        IngredientId = it.IngredientId,
                        Quantity = (decimal)it.Quantity,
                        Unit = it.Unit ?? "",
                        ExpiryDate = null,
                        AddedAt = DateTime.UtcNow
                    });
                }

                await _context.SaveChangesAsync();

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
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
