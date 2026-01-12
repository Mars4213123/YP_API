using Microsoft.AspNetCore.Mvc;
using YP_API.Data;
using YP_API.Models;

[ApiController]
[Route("api/[controller]")]
public class UserPreferencesController : ControllerBase
{
    private readonly RecipePlannerContext _context;
    public UserPreferencesController(RecipePlannerContext context) => _context = context;

    // POST api/userpreferences/{userId}/allergies
    [HttpPost("{userId}/allergies")]
    public async Task<ActionResult> SetAllergies(int userId, [FromBody] int[] ingredientIds)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound(new { error = "Пользователь не найден" });

        var old = _context.UserAllergies.Where(a => a.UserId == userId);
        _context.UserAllergies.RemoveRange(old);

        foreach (var id in ingredientIds.Distinct())
            _context.UserAllergies.Add(new UserAllergy { UserId = userId, IngredientId = id });

        await _context.SaveChangesAsync();
        return Ok(new { success = true });
    }

    // POST api/userpreferences/{userId}/fridge
    [HttpPost("{userId}/fridge")]
    public async Task<ActionResult> SetFridge(int userId, [FromBody] List<FridgeItemDto> items)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound(new { error = "Пользователь не найден" });

        var old = _context.FridgeItems.Where(f => f.UserId == userId);
        _context.FridgeItems.RemoveRange(old);

        foreach (var it in items)
            _context.FridgeItems.Add(new FridgeItem
            {
                UserId = userId,
                IngredientId = it.IngredientId,
                Quantity = it.Quantity
            });

        await _context.SaveChangesAsync();
        return Ok(new { success = true });
    }

    public class FridgeItemDto
    {
        public int IngredientId { get; set; }
        public double Quantity { get; set; }
    }
}
