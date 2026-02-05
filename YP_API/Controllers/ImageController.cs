using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YP_API.Data;
using YP_API.Models;
using YP_API.Services;

namespace YP_API.Controllers
{
    public class RecipeImageDto
    {
        public string query { get; set; }
        public int RecipeId { get; set; }   
    }

    
    [ApiController]
    [Route("api/[controller]")]
    public class ImagesController : ControllerBase
    {
        private readonly IPovarScraperService _povarService;

        private readonly RecipePlannerContext _context;

        public ImagesController(IPovarScraperService povarService, RecipePlannerContext context)
        {
            _povarService = povarService;
            _context = context;
        }

        /// <summary>
        /// Парсит картинку с сайта Povar.ru по названию блюда
        /// </summary>
        [HttpGet("parse-povar")]
        public async Task<IActionResult> ParseImage([FromQuery] RecipeImageDto recipeImage)
        {
            if (string.IsNullOrWhiteSpace(recipeImage.query))
                return BadRequest("Запрос не может быть пустым");

            var imageUrl = await _povarService.FindImageAsync(recipeImage.query);

            if (imageUrl == null)
            {
                return NotFound(new { message = "Картинка не найдена или сайт недоступен" });
            }

            var recepie = await _context.Recipes.FirstOrDefaultAsync(x => x.Id == recipeImage.RecipeId);

            if (recepie == null)
                return NotFound(new { message = "Рецепт не найден" });

            recepie.ImageUrl = imageUrl;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                query = recipeImage.query,
                source = "povar.ru",
                url = imageUrl
            });
        }
    }
}