using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YP_API.DTOs;
using YP_API.Interfaces;
using YP_API.Models;

namespace YP_API.Controllers
{
    public class MenuController : BaseApiController
    {
        private readonly IMenuRepository _menuRepository;
        private readonly IRecipeRepository _recipeRepository;
        private readonly IUserRepository _userRepository;

        public MenuController(IMenuRepository menuRepository, IRecipeRepository recipeRepository, IUserRepository userRepository)
        {
            _menuRepository = menuRepository;
            _recipeRepository = recipeRepository;
            _userRepository = userRepository;
        }

        [HttpGet("current")]
        [Authorize]
        public async Task<ActionResult<MenuPlan>> GetCurrentMenu()
        {
            var userId = GetUserId();
            var menu = await _menuRepository.GetCurrentMenuAsync(userId);

            if (menu == null)
                return NotFound("No current menu found");

            return Ok(menu);
        }

        [HttpPost("generate")]
        [Authorize]
        public async Task<ActionResult<MenuPlan>> GenerateMenu([FromBody] GenerateMenuRequestDto request)
        {
            var userId = GetUserId();

            var userAllergies = await _userRepository.GetUserAllergiesAsync(userId);
            var allergyCodes = userAllergies.Select(a => a.Code).ToList();

            var availableRecipes = await _recipeRepository.GetRecipesByAllergiesAsync(allergyCodes);

            var random = new Random();
            var menuPlan = new MenuPlan
            {
                UserId = userId,
                Name = $"Menu Plan {DateTime.Today:yyyy-MM-dd}",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(request.Days - 1),
                CreatedAt = DateTime.UtcNow
            };

            for (int i = 0; i < request.Days; i++)
            {
                var date = DateTime.Today.AddDays(i);

                var breakfast = availableRecipes
                    .Where(r => r.RecipeTags.Any(t => t.Tag == "breakfast" || t.Tag == "easy"))
                    .OrderBy(r => random.Next())
                    .FirstOrDefault();

                if (breakfast != null)
                {
                    menuPlan.MenuPlanItems.Add(new MenuPlanItem
                    {
                        Date = date,
                        MealType = MealType.Breakfast,
                        RecipeId = breakfast.Id
                    });
                }

                var lunch = availableRecipes
                    .Where(r => r.RecipeTags.Any(t => t.Tag == "lunch" || t.Tag == "main"))
                    .OrderBy(r => random.Next())
                    .FirstOrDefault();

                if (lunch != null)
                {
                    menuPlan.MenuPlanItems.Add(new MenuPlanItem
                    {
                        Date = date,
                        MealType = MealType.Lunch,
                        RecipeId = lunch.Id
                    });
                }

                var dinner = availableRecipes
                    .Where(r => r.RecipeTags.Any(t => t.Tag == "dinner" || t.Tag == "main"))
                    .OrderBy(r => random.Next())
                    .FirstOrDefault();

                if (dinner != null)
                {
                    menuPlan.MenuPlanItems.Add(new MenuPlanItem
                    {
                        Date = date,
                        MealType = MealType.Dinner,
                        RecipeId = dinner.Id
                    });
                }
            }

            menuPlan.TotalCalories = (int)menuPlan.MenuPlanItems.Sum(item =>
                item.Recipe?.Calories ?? availableRecipes.First(r => r.Id == item.RecipeId).Calories);

            var createdMenu = await _menuRepository.CreateMenuAsync(menuPlan);
            await _menuRepository.SaveAllAsync();

            return Ok(createdMenu);
        }
    }
}
