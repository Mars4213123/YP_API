using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using YP_API.Interfaces;
using YP_API.Models;
using YP_API.Services;

namespace YP_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShoppingListController : ControllerBase
    {
        private readonly IShoppingListService _shoppingListService;

        public ShoppingListController(IShoppingListService shoppingListService)
        {
            _shoppingListService = shoppingListService;
        }

        [HttpGet("current/{userId}")]
        public async Task<ActionResult> GetCurrentShoppingList(int userId)
        {
            var shoppingList = await _shoppingListService.GetCurrentShoppingListAsync(userId);

            if (shoppingList == null)
                return NotFound(new { error = "Текущий список покупок не найден" });

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
        public async Task<ActionResult> ToggleItemPurchased(
            int listId,
            int itemId,
            [FromForm]
            [Required(ErrorMessage = "Статус покупки обязателен")]
            [Display(Name = "Статус покупки (куплено/не куплено)")]
            bool isPurchased)
        {
            var success = await _shoppingListService.ToggleItemPurchasedAsync(itemId, isPurchased);

            if (success)
                return Ok(new { message = "Статус покупки успешно изменен" });

            return BadRequest(new { error = "Не удалось изменить статус покупки" });
        }

        [HttpPost("generate-from-menu/{menuId}/{userId}")]
        public async Task<ActionResult> GenerateFromMenu(int menuId, int userId)
        {
            try
            {
                var shoppingList = await _shoppingListService.GenerateShoppingListFromMenuAsync(menuId, userId);

                return Ok(new
                {
                    Id = shoppingList.Id,
                    Name = shoppingList.Name,
                    Message = "Список покупок успешно сгенерирован",
                    ItemsCount = shoppingList.Items?.Count ?? 0
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    error = "Ошибка генерации списка покупок",
                    message = ex.Message
                });
            }
        }
    }
}