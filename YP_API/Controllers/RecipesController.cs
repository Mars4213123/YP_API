using Microsoft.AspNetCore.Mvc;
using YP_API.Interfaces;
using YP_API.Models;

namespace YP_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecipesController : ControllerBase
    {
        private readonly IRecipeRepository _recipeRepository;

        public RecipesController(IRecipeRepository recipeRepository)
        {
            _recipeRepository = recipeRepository;
        }

        [HttpGet]
        public async Task<ActionResult> GetRecipes()
        {
            try
            {
                var recipes = await _recipeRepository.GetAllAsync();

                return Ok(new
                {
                    success = true,
                    data = recipes.Select(r => new {
                        Id = r.Id,
                        Title = r.Title,
                        Description = r.Description,
                        Calories = r.Calories,
                        ImageUrl = r.ImageUrl
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetRecipe(int id)
        {
            try
            {
                var recipe = await _recipeRepository.GetByIdAsync(id);

                if (recipe == null)
                    return NotFound(new { error = "Рецепт не найден" });

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        Id = recipe.Id,
                        Title = recipe.Title,
                        Description = recipe.Description,
                        Instructions = recipe.Instructions,
                        Calories = recipe.Calories,
                        ImageUrl = recipe.ImageUrl
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("favorites/{userId}")]
        public async Task<ActionResult> GetFavorites(int userId)
        {
            try
            {
                var favorites = await _recipeRepository.GetUserFavoritesAsync(userId);

                return Ok(new
                {
                    success = true,
                    data = favorites.Select(r => new {
                        Id = r.Id,
                        Title = r.Title,
                        Description = r.Description,
                        Calories = r.Calories,
                        ImageUrl = r.ImageUrl
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("{id}/favorite/{userId}")]
        public async Task<ActionResult> ToggleFavorite(int id, int userId)
        {
            try
            {
                var success = await _recipeRepository.ToggleFavoriteAsync(userId, id);

                return Ok(new
                {
                    success = true,
                    message = success ? "Добавлено в избранное" : "Удалено из избранного"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}