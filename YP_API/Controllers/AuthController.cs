using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using YP_API.Models;
using YP_API.Services;

namespace YP_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(
            [FromForm]
            [Required(ErrorMessage = "Имя пользователя обязательно")]
            [StringLength(50, MinimumLength = 3, ErrorMessage = "Имя пользователя должно быть от 3 до 50 символов")]
            [Display(Name = "Имя пользователя")]
            string username,

            [FromForm]
            [Required(ErrorMessage = "Email обязателен")]
            [EmailAddress(ErrorMessage = "Неверный формат email")]
            [Display(Name = "Email адрес")]
            string email,

            [FromForm]
            [Required(ErrorMessage = "Полное имя обязательно")]
            [StringLength(100, ErrorMessage = "Полное имя не должно превышать 100 символов")]
            [Display(Name = "Полное имя")]
            string fullName,

            [FromForm]
            [Required(ErrorMessage = "Пароль обязателен")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть не менее 6 символов")]
            [Display(Name = "Пароль")]
            string password,

            [FromForm]
            [Display(Name = "Аллергии")]
            List<string> allergies = null)
        {
            try
            {
                var user = await _authService.Register(username, email, fullName, password, allergies ?? new List<string>());

                return Ok(new
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    Message = "Пользователь успешно создан"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(
            [FromForm]
            [Required(ErrorMessage = "Имя пользователя обязательно")]
            [Display(Name = "Имя пользователя")]
            string username,

            [FromForm]
            [Required(ErrorMessage = "Пароль обязателен")]
            [Display(Name = "Пароль")]
            string password)
        {
            try
            {
                var user = await _authService.Login(username, password);

                return Ok(new
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    Message = "Пользователь успешно найден"
                });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
        }
    }
}