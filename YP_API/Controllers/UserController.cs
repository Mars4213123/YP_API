using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YP_API.Interfaces;
using YP_API.Models;
using System.Security.Claims;
using YP_API.Models.Dtos;
using System.Collections.Generic; // Для List<string>
using System.Linq; // Для Select

namespace YP_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Весь контроллер требует авторизации
    public class UserController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserRepository userRepository, ILogger<UserController> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        // GET api/User/profile
        [HttpGet("profile")]
        public async Task<ActionResult> GetUserProfile()
        {
            try
            {
                var userId = GetUserId(); // Метод из BaseApiController
                var user = await _userRepository.GetByIdAsync(userId);

                if (user == null)
                {
                    return NotFound(new { success = false, error = "User not found" });
                }

                // 🟢 ИСПРАВЛЕНО: user.Allergies используется напрямую, т.к. является List<string>
                var userProfile = new
                {
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    CreatedAt = user.CreatedAt,
                    Allergies = user.Allergies ?? new List<string>()
                };

                return Ok(userProfile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        // POST api/User/profile
        [HttpPost("profile")]
        public async Task<ActionResult> UpdateUserProfile(
            [FromForm] string fullName,
            [FromForm] string email)
        {
            try
            {
                var userId = GetUserId();
                var user = await _userRepository.GetByIdAsync(userId);

                if (user == null)
                {
                    return NotFound(new { success = false, error = "User not found" });
                }

                user.FullName = fullName;
                user.Email = email;

                _userRepository.Update(user);
                if (await _userRepository.SaveAllAsync())
                {
                    return Ok(new { success = true, message = "Profile updated" });
                }
                return BadRequest(new { success = false, error = "Failed to update profile" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        // 🟢 ИСПРАВЛЕННЫЙ МЕТОД: GET api/User/allergies
        [HttpGet("allergies")]
        public async Task<ActionResult<List<string>>> GetUserAllergies()
        {
            try
            {
                var userId = GetUserId();
                var user = await _userRepository.GetByIdAsync(userId);

                if (user == null)
                {
                    return NotFound(new { success = false, error = "User not found" });
                }

                // 🟢 ИСПРАВЛЕНО: user.Allergies используется напрямую
                return Ok(user.Allergies ?? new List<string>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user allergies");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        // 🟢 ИСПРАВЛЕННЫЙ МЕТОД: POST api/User/allergies
        [HttpPost("allergies")]
        public async Task<ActionResult> UpdateUserAllergies([FromBody] UpdateAllergiesDto updateDto)
        {
            try
            {
                var userId = GetUserId();
                var user = await _userRepository.GetByIdAsync(userId);

                if (user == null)
                {
                    return NotFound(new { success = false, error = "User not found" });
                }

                // 🟢 ИСПРАВЛЕНО: Прямое присвоение List<string> к user.Allergies
                // Это работает, если user.Allergies в модели User также является List<string>.
                user.Allergies = updateDto.Allergies ?? new List<string>();

                _userRepository.Update(user);
                if (await _userRepository.SaveAllAsync())
                {
                    return Ok(new { success = true, message = "Allergies updated" });
                }
                return BadRequest(new { success = false, error = "Failed to update allergies" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user allergies");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
    }
}