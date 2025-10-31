using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YP_API.DTOs;
using YP_API.Helpers;
using YP_API.Interfaces;
using YP_API.Models;
using YP_API.Services;

namespace YP_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ShoppingListController : ControllerBase
    {
        private readonly IShoppingListService _shoppingListService;

        public ShoppingListController(IShoppingListService shoppingListService)
        {
            _shoppingListService = shoppingListService;
        }

        [HttpGet("current")]
        public async Task<ActionResult<ShoppingListDto>> GetCurrentShoppingList()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            var shoppingList = await _shoppingListService.GetCurrentShoppingListAsync(userId);

            if (shoppingList == null)
                return NotFound("No current shopping list found");

            return Ok(shoppingList);
        }

        [HttpPost("{listId}/items/{itemId}/toggle")]
        public async Task<ActionResult> ToggleItemPurchased(int listId, int itemId, [FromBody] UpdateShoppingItemDto updateDto)
        {
            var success = await _shoppingListService.ToggleItemPurchasedAsync(itemId, updateDto.IsPurchased);

            if (success)
                return Ok();

            return BadRequest("Failed to update item");
        }

        [HttpPost("generate-from-menu/{menuId}")]
        public async Task<ActionResult<ShoppingListDto>> GenerateFromMenu(int menuId)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            var shoppingList = await _shoppingListService.GenerateShoppingListFromMenuAsync(menuId, userId);
            return Ok(shoppingList);
        }
    }
}
