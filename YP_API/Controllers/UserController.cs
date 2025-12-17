using Microsoft.AspNetCore.Mvc;
using YP_API.Interfaces;
using YP_API.Models.Dtos;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserRepository userRepository, ILogger<UserController> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    [HttpGet("profile/{userId}")]
    public async Task<ActionResult> GetUserProfile(int userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new { success = false, error = "User not found" });
            }

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

    [HttpPost("profile/{userId}")]
    public async Task<ActionResult> UpdateUserProfile(
        int userId,
        [FromForm] string fullName,
        [FromForm] string email)
    {
        try
        {
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

    [HttpGet("allergies/{userId}")]
    public async Task<ActionResult<List<string>>> GetUserAllergies(int userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new { success = false, error = "User not found" });
            }

            return Ok(user.Allergies ?? new List<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user allergies");
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    [HttpPost("allergies/{userId}")]
    public async Task<ActionResult> UpdateUserAllergies(
        int userId,
        [FromBody] UpdateAllergiesDto updateDto)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new { success = false, error = "User not found" });
            }

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