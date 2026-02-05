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
        private readonly YP_API.Interfaces.IMenuService _menuService;

        public InventoryController(RecipePlannerContext context, YP_API.Interfaces.IMenuService menuService)
        {
            _context = context;
            _menuService = menuService;
        }


        [HttpPost("FridgeItem/add/{userId}")]
        public async Task<IActionResult> AddFridgeItem(int userId, [FromBody] Ingredient ingredient)
        {

            try
            {
                var existingIngredient = await _context.Ingredients.FindAsync(ingredient.Id);

                if (existingIngredient == null)
                {
                    if (string.IsNullOrEmpty(ingredient.Category))
                    {
                        ingredient.Category = "Разное";
                    }

                    _context.Ingredients.Add(ingredient);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    ingredient = existingIngredient;
                }

                var fridgeIngredient = new FridgeItem
                {
                    IngredientId = ingredient.Id,
                    UserId = userId,
                    ProductName = ingredient.Name,
                    Quantity = 1,
                    Unit = ingredient.Unit ?? "шт"
                };

                _context.FridgeItems.Add(fridgeIngredient);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, menuGenerated = false });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // УСТАНОВИТЬ инвентарь (заменить старый)
        [HttpPost("set/{userId}")]
        public async Task<IActionResult> SetInventory(int userId, [FromBody] List<InventoryItemDto> items)
        {
            try
            {
                // Очищаем старый инвентарь
                var oldInventory = await _context.UserInventories
                    .Where(ui => ui.UserId == userId)
                    .ToListAsync();
                
                _context.UserInventories.RemoveRange(oldInventory);
                await _context.SaveChangesAsync();

                // Добавляем новый инвентарь
                foreach (var item in items)
                {
                    var ingredient = await _context.Ingredients.FindAsync(item.IngredientId);
                    if (ingredient == null)
                    {
                        return BadRequest(new { error = $"Ингредиент с ID {item.IngredientId} не найден" });
                    }

                    _context.UserInventories.Add(new UserInventory
                    {
                        UserId = userId,
                        IngredientId = item.IngredientId,
                        Quantity = item.Quantity,
                        Unit = item.Unit ?? "шт"
                    });
                }

                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Инвентарь обновлен" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        public class InventoryItemDto
        {
            public int IngredientId { get; set; }
            public decimal Quantity { get; set; }
            public string? Unit { get; set; }
        }
    }
}
