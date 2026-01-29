using Microsoft.AspNetCore.Mvc;
using YP_API.Interfaces;
using YP_API.Models;
using Microsoft.EntityFrameworkCore;
using YP_API.Data;


namespace YP_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecipesController : ControllerBase
    {
        private readonly IRecipeRepository _recipeRepository;
        private readonly RecipePlannerContext _context;

        public RecipesController(IRecipeRepository recipeRepository, RecipePlannerContext context)
        {
            _recipeRepository = recipeRepository;
            _context = context;
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


        [HttpGet("safe-for-user/{userId}")]
        public async Task<ActionResult> GetSafeRecipes(int userId)
        {
            try
            {
                var allergyIngredientIds = await _context.UserAllergies
                    .Where(a => a.UserId == userId)
                    .Select(a => a.IngredientId)
                    .ToListAsync();

                var safeRecipes = await _context.Recipes
                    .Include(r => r.RecipeIngredients)
                    .Where(r => !r.RecipeIngredients
                        .Any(ri => allergyIngredientIds.Contains(ri.IngredientId)))
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = safeRecipes.Select(r => new
                    {
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


        // GET api/recipes/by-fridge/{userId}
        [HttpGet("by-fridge/{userId}")]
        public async Task<ActionResult> GetRecipesByFridge(int userId)
        {
            try
            {
                var fridgeItems = await _context.UserInventories
            .Where(ui => ui.UserId == userId)
            .ToListAsync();
                Console.WriteLine($"DEBUG: User {userId} has {fridgeItems.Count} items in fridge.");
                foreach (var item in fridgeItems)
                {
                    Console.WriteLine($"DEBUG: Item IngId={item.IngredientId}, Qty={item.Quantity}");
                }

                var fridgeIngredientIds = fridgeItems
                    .Select(ui => ui.IngredientId)
                    .Distinct()
                    .ToList();

                if (!fridgeIngredientIds.Any())
                {
                    Console.WriteLine("DEBUG: Fridge is empty!");
                    return Ok(new { success = true, data = Array.Empty<object>(), message = "В холодильнике нет продуктов" });
                }

                var recipeIdsQuery =
    from r in _context.Recipes
    where _context.RecipeIngredients
        .Where(ri => ri.RecipeId == r.Id)
        .Any(ri => fridgeIngredientIds.Contains(ri.IngredientId)) // <--- Мягкий поиск (хотя бы один)
    select r.Id;


                var recipeIds = await recipeIdsQuery.ToListAsync();

                var recipes = await _context.Recipes
                    .Where(r => recipeIds.Contains(r.Id))
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = recipes.Select(r => new
                    {
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

        // DEBUG: информация по холодильнику и совпадениям рецептов
        // GET api/recipes/debug/fridge-info/{userId}
        [HttpGet("debug/fridge-info/{userId}")]
        public async Task<ActionResult> GetFridgeDebugInfo(int userId)
        {
            try
            {
                var fridgeItems = await _context.UserInventories
                    .Where(ui => ui.UserId == userId)
                    .Include(ui => ui.Ingredient)
                    .ToListAsync();

                var fridgeIds = fridgeItems.Select(fi => fi.IngredientId).Distinct().ToList();
                var fridgeNames = fridgeItems.Select(fi => fi.Ingredient?.Name?.ToLower()).Where(n => !string.IsNullOrEmpty(n)).Distinct().ToList();

                var matched = await (from r in _context.Recipes
                                     join ri in _context.RecipeIngredients on r.Id equals ri.RecipeId
                                     join ing in _context.Ingredients on ri.IngredientId equals ing.Id
                                     where fridgeIds.Contains(ri.IngredientId) || fridgeNames.Contains(ing.Name.ToLower())
                                     select new
                                     {
                                         RecipeId = r.Id,
                                         RecipeTitle = r.Title,
                                         IngredientId = ri.IngredientId,
                                         IngredientName = ing.Name
                                     })
                                    .ToListAsync();

                var recipes = matched.Select(m => new { m.RecipeId, m.RecipeTitle }).Distinct().ToList();

                return Ok(new
                {
                    success = true,
                    fridge = fridgeItems.Select(fi => new { fi.IngredientId, Name = fi.Ingredient?.Name, fi.Quantity }),
                    matchedRecipes = recipes,
                    matchedDetails = matched
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

    }



}
