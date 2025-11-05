using System.ComponentModel.DataAnnotations;
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
        public async Task<ActionResult> GetRecipes([FromQuery] RecipeSearchParams searchParams)
        {
            try
            {
                var recipes = await _recipeRepository.GetRecipesAsync(searchParams);

                Response.Headers.Add("X-Pagination", System.Text.Json.JsonSerializer.Serialize(new
                {
                    recipes.CurrentPage,
                    recipes.PageSize,
                    recipes.TotalCount,
                    recipes.TotalPages
                }));

                return Ok(recipes.Select(r => new {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
                    PrepTime = r.PrepTime,
                    CookTime = r.CookTime,
                    Calories = r.Calories,
                    ImageUrl = r.ImageUrl,
                    Difficulty = r.Difficulty
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetRecipe(int id)
        {
            try
            {
                var recipe = await _recipeRepository.GetRecipeWithDetailsAsync(id);

                if (recipe == null)
                    return NotFound(new { error = "Recipe not found" });

                return Ok(new
                {
                    Id = recipe.Id,
                    Title = recipe.Title,
                    Description = recipe.Description,
                    Instructions = recipe.Instructions,
                    PrepTime = recipe.PrepTime,
                    CookTime = recipe.CookTime,
                    Servings = recipe.Servings,
                    Calories = recipe.Calories,
                    ImageUrl = recipe.ImageUrl,
                    Difficulty = recipe.Difficulty,
                    CuisineType = recipe.CuisineType,
                    Ingredients = recipe.RecipeIngredients?.Select(ri => new {
                        Name = ri.Ingredient?.Name,
                        Quantity = ri.Quantity,
                        Unit = ri.Unit,
                        Category = ri.Ingredient?.Category
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> CreateRecipe(
            [FromForm] string title,
            [FromForm] string description,
            [FromForm] string instructions,
            [FromForm] int prepTime,
            [FromForm] int cookTime,
            [FromForm] int servings,
            [FromForm] decimal calories,
            [FromForm] string imageUrl = "",
            [FromForm] string cuisineType = "",
            [FromForm] string difficulty = "")
        {
            try
            {
                var recipe = new Recipe
                {
                    Title = title,
                    Description = description,
                    Instructions = instructions,
                    PrepTime = prepTime,
                    CookTime = cookTime,
                    Servings = servings,
                    Calories = calories,
                    ImageUrl = imageUrl,
                    CuisineType = cuisineType,
                    Difficulty = difficulty,
                    CreatedAt = DateTime.UtcNow
                };

                await _recipeRepository.AddAsync(recipe);

                if (await _recipeRepository.SaveAllAsync())
                {
                    return Ok(new
                    {
                        Id = recipe.Id,
                        Message = "Recipe created successfully",
                        Title = recipe.Title
                    });
                }

                return BadRequest(new { error = "Failed to create recipe" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpPost("{id}/favorite")]
        [Authorize]
        public async Task<ActionResult> ToggleFavorite(int id)
        {
            try
            {
                var userId = GetUserId();
                var success = await _recipeRepository.ToggleFavoriteAsync(userId, id);

                if (success)
                    return Ok(new { message = "Favorite toggled successfully" });

                return BadRequest(new { error = "Failed to toggle favorite" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpGet("favorites")]
        [Authorize]
        public async Task<ActionResult> GetFavorites()
        {
            try
            {
                var userId = GetUserId();
                var favorites = await _recipeRepository.GetUserFavoritesAsync(userId);

                return Ok(favorites.Select(r => new {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
                    Calories = r.Calories,
                    ImageUrl = r.ImageUrl,
                    PrepTime = r.PrepTime,
                    CookTime = r.CookTime
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }
    }
}

