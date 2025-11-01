using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YP_API.DTOs;
using YP_API.Helpers;
using YP_API.Interfaces;
using YP_API.Models;

namespace YP_API.Controllers
{
    public class RecipesController : BaseApiController
    {
        private readonly IRecipeRepository _recipeRepository;

        public RecipesController(IRecipeRepository recipeRepository)
        {
            _recipeRepository = recipeRepository;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PagedList<RecipeDto>>> GetRecipes([FromQuery] RecipeSearchParams searchParams)
        {
            var recipes = await _recipeRepository.GetRecipesAsync(searchParams);

            Response.Headers.Add("X-Pagination", System.Text.Json.JsonSerializer.Serialize(new
            {
                recipes.CurrentPage,
                recipes.PageSize,
                recipes.TotalCount,
                recipes.TotalPages
            }));

            return Ok(recipes);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<RecipeDto>> GetRecipe(int id)
        {
            var recipe = await _recipeRepository.GetRecipeDtoByIdAsync(id);

            if (recipe == null)
                return NotFound();

            return Ok(recipe);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<RecipeDto>> CreateRecipe(CreateRecipeDto createRecipeDto)
        {
            var recipe = await _recipeRepository.CreateRecipeAsync(createRecipeDto);

            if (await _recipeRepository.SaveAllAsync())
            {
                var recipeDto = await _recipeRepository.GetRecipeDtoByIdAsync(recipe.Id);
                return CreatedAtAction(nameof(GetRecipe), new { id = recipe.Id }, recipeDto);
            }

            return BadRequest("Failed to create recipe");
        }

        [HttpPost("{id}/favorite")]
        [Authorize]
        public async Task<ActionResult> ToggleFavorite(int id)
        {
            var userId = GetUserId();
            var success = await _recipeRepository.ToggleFavoriteAsync(userId, id);

            if (success)
                return Ok();

            return BadRequest("Failed to toggle favorite");
        }

        [HttpGet("favorites")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<RecipeDto>>> GetFavorites()
        {
            var userId = GetUserId();
            var favorites = await _recipeRepository.GetUserFavoritesAsync(userId);
            return Ok(favorites);
        }
    }
}

