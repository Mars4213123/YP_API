using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<ActionResult<PagedList<Recipe>>> GetRecipes([FromQuery] RecipeSearchParams searchParams)
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
        public async Task<ActionResult<Recipe>> GetRecipe(int id)
        {
            var recipe = await _recipeRepository.GetByIdAsync(id);
            
            if (recipe == null)
                return NotFound();
                
            return Ok(recipe);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Recipe>> CreateRecipe(Recipe recipe)
        {
            await _recipeRepository.AddRecipeAsync(recipe);
            
            if (await _recipeRepository.SaveAllAsync())
                return CreatedAtAction(nameof(GetRecipe), new { id = recipe.Id }, recipe);
                
            return BadRequest("Failed to create recipe");
        }
    }
}
