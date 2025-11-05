using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
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
            [FromForm] string username,
            [FromForm] string email,
            [FromForm] string fullName,
            [FromForm] string password,
            [FromForm] List<string> allergies = null)
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
                    Token = user.Token
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(
            [FromForm] string username,
            [FromForm] string password)
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
                    Token = user.Token
                });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
        }
    }
}

