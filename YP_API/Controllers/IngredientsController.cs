using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YP_API.Helpers;
using YP_API.Interfaces;
using YP_API.Models;

namespace YP_API.Controllers
{
    public class IngredientsController : BaseApiController
    {
        private readonly IIngredientRepository _ingredientRepository;

        public IngredientsController(IIngredientRepository ingredientRepository)
        {
            _ingredientRepository = ingredientRepository;
        }

        [HttpGet("categories")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<IngredientCategory>>> GetCategories()
        {
            var categories = await _ingredientRepository.GetCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PagedList<Ingredient>>> GetIngredients([FromQuery] IngredientSearchParams searchParams)
        {
            var ingredients = await _ingredientRepository.GetIngredientsAsync(searchParams);

            Response.Headers.Add("X-Pagination", System.Text.Json.JsonSerializer.Serialize(new
            {
                ingredients.CurrentPage,
                ingredients.PageSize,
                ingredients.TotalCount,
                ingredients.TotalPages
            }));

            return Ok(ingredients);
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Ingredient>>> SearchIngredients([FromQuery] string query, [FromQuery] int? categoryId = null)
        {
            var ingredients = await _ingredientRepository.SearchIngredientsAsync(query, categoryId);
            return Ok(ingredients);
        }
    }
}
