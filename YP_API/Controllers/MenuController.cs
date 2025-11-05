using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using YP_API.Interfaces;
using YP_API.Models;
using YP_API.Services;

namespace YP_API.Controllers
{
    public class MenuController : BaseApiController
    {
        private readonly IMenuService _menuService;
        private readonly IUserRepository _userRepository;

        public MenuController(IMenuService menuService, IUserRepository userRepository)
        {
            _menuService = menuService;
            _userRepository = userRepository;
        }

        [HttpGet("current")]
        public async Task<ActionResult> GetCurrentMenu()
        {
            var userId = GetUserId();
            var menu = await _menuService.GetCurrentMenuAsync(userId);

            if (menu == null)
                return NotFound(new { error = "No current menu found" });

            return Ok(new
            {
                Id = menu.Id,
                Name = menu.Name,
                StartDate = menu.StartDate,
                EndDate = menu.EndDate,
                TotalCalories = menu.TotalCalories,
                Days = menu.MenuMeals?.GroupBy(m => m.MealDate).Select(g => new {
                    Date = g.Key,
                    Meals = g.Select(m => new {
                        Id = m.Id,
                        RecipeId = m.RecipeId,
                        RecipeTitle = m.Recipe?.Title,
                        MealType = m.MealType,
                        Calories = m.Recipe?.Calories,
                        PrepTime = (m.Recipe?.PrepTime ?? 0) + (m.Recipe?.CookTime ?? 0),
                        ImageUrl = m.Recipe?.ImageUrl
                    })
                })
            });
        }

        [HttpPost("generate")]
        public async Task<ActionResult> GenerateMenu(
            [FromForm] int days = 7,
            [FromForm] decimal? targetCaloriesPerDay = null,
            [FromForm] List<string> cuisineTags = null,
            [FromForm] List<string> mealTypes = null,
            [FromForm] bool useInventory = false)
        {
            var userId = GetUserId();
            var user = await _userRepository.GetByIdAsync(userId);

            var request = new GenerateMenuRequest
            {
                Days = days,
                TargetCaloriesPerDay = targetCaloriesPerDay,
                CuisineTags = cuisineTags ?? new List<string>(),
                MealTypes = mealTypes ?? new List<string> { "breakfast", "lunch", "dinner" },
                UseInventory = useInventory
            };

            var menu = await _menuService.GenerateWeeklyMenuAsync(userId, request, user?.Allergies);

            return Ok(new
            {
                Id = menu.Id,
                Name = menu.Name,
                StartDate = menu.StartDate,
                EndDate = menu.EndDate,
                TotalCalories = menu.TotalCalories,
                Message = "Menu generated successfully"
            });
        }

        [HttpGet("history")]
        public async Task<ActionResult> GetMenuHistory()
        {
            var userId = GetUserId();
            var menus = await _menuService.GetUserMenuHistoryAsync(userId);

            return Ok(menus.Select(m => new {
                Id = m.Id,
                Name = m.Name,
                StartDate = m.StartDate,
                EndDate = m.EndDate,
                TotalCalories = m.TotalCalories
            }));
        }

        [HttpPost("{menuId}/regenerate-day")]
        public async Task<ActionResult> RegenerateDay(int menuId, [FromForm] DateTime date)
        {
            var userId = GetUserId();
            var user = await _userRepository.GetByIdAsync(userId);

            var menu = await _menuService.RegenerateDayAsync(menuId, date, user?.Allergies);

            return Ok(new
            {
                Message = "Day regenerated successfully",
                MenuId = menu.Id,
                Date = date
            });
        }
    }

    public class GenerateMenuRequest
    {
        public int Days { get; set; } = 7;
        public decimal? TargetCaloriesPerDay { get; set; }
        public List<string> CuisineTags { get; set; } = new List<string>();
        public List<string> MealTypes { get; set; } = new List<string> { "breakfast", "lunch", "dinner" };
        public bool UseInventory { get; set; } = false;
    }
}

