using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YP_API.Helpers;
using YP_API.Interfaces;
using YP_API.Models;
using YP_API.Services;

namespace YP_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ShoppingListController : BaseApiController
    {
        private readonly IShoppingListService _shoppingListService;

        public ShoppingListController(IShoppingListService shoppingListService)
        {
            _shoppingListService = shoppingListService;
        }

        [HttpGet("current")]
        public async Task<ActionResult> GetCurrentShoppingList()
        {
            var userId = GetUserId();
            var shoppingList = await _shoppingListService.GetCurrentShoppingListAsync(userId);

            if (shoppingList == null)
                return NotFound(new { error = "No current shopping list found" });

            return Ok(new
            {
                Id = shoppingList.Id,
                Name = shoppingList.Name,
                IsCompleted = shoppingList.IsCompleted,
                Items = shoppingList.Items?.Select(i => new {
                    Id = i.Id,
                    IngredientName = i.Ingredient?.Name,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    Category = i.Category,
                    IsPurchased = i.IsPurchased
                })
            });
        }

        [HttpPost("{listId}/items/{itemId}/toggle")]
        public async Task<ActionResult> ToggleItemPurchased(int listId, int itemId, [FromForm] bool isPurchased)
        {
            var success = await _shoppingListService.ToggleItemPurchasedAsync(itemId, isPurchased);

            if (success)
                return Ok(new { message = "Item updated successfully" });

            return BadRequest(new { error = "Failed to update item" });
        }

        [HttpPost("generate-from-menu/{menuId}")]
        public async Task<ActionResult> GenerateFromMenu(int menuId)
        {
            var userId = GetUserId();
            var shoppingList = await _shoppingListService.GenerateShoppingListFromMenuAsync(menuId, userId);

            return Ok(new
            {
                Id = shoppingList.Id,
                Name = shoppingList.Name,
                Message = "Shopping list generated successfully",
                ItemsCount = shoppingList.Items?.Count ?? 0
            });
        }
    }
}

