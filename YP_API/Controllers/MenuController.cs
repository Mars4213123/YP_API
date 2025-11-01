using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YP_API.DTOs;
using YP_API.Interfaces;
using YP_API.Models;
using YP_API.Repositories;
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
        public async Task<ActionResult<WeeklyMenuDto>> GetCurrentMenu()
        {
            var userId = GetUserId();
            var menu = await _menuService.GetCurrentMenuAsync(userId);

            if (menu == null)
                return NotFound("No current menu found");

            return Ok(menu);
        }

        [HttpPost("generate")]
        public async Task<ActionResult<WeeklyMenuDto>> GenerateMenu([FromBody] GenerateMenuRequestDto request)
        {
            var userId = GetUserId();
            var user = await _userRepository.GetByIdAsync(userId);

            var menu = await _menuService.GenerateWeeklyMenuAsync(userId, request, user?.Allergies);
            return Ok(menu);
        }

        [HttpGet("history")]
        public async Task<ActionResult<IEnumerable<WeeklyMenuDto>>> GetMenuHistory()
        {
            var userId = GetUserId();
            var menus = await _menuService.GetUserMenuHistoryAsync(userId);
            return Ok(menus);
        }

        [HttpPost("{menuId}/regenerate-day")]
        public async Task<ActionResult<MenuDayDto>> RegenerateDay(int menuId, [FromBody] RegenerateDayRequestDto request)
        {
            var userId = GetUserId();
            var user = await _userRepository.GetByIdAsync(userId);

            var day = await _menuService.RegenerateDayAsync(menuId, request.Date, user?.Allergies);
            return Ok(day);
        }
    }

    public class RegenerateDayRequestDto
    {
        public DateTime Date { get; set; }
    }
}

