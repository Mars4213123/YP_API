using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace YP_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BaseApiController : ControllerBase
    {
        protected int GetUserId()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                               ?? User.FindFirst("nameid")?.Value
                               ?? User.FindFirst("sub")?.Value
                               ?? User.FindFirst("userId")?.Value;

                if (string.IsNullOrEmpty(userIdClaim))
                {
                    var allClaims = User.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
                    Console.WriteLine("Available claims: " + string.Join(", ", allClaims));

                    throw new UnauthorizedAccessException("User ID not found in token. Available claims: " + string.Join(", ", allClaims));
                }

                if (int.TryParse(userIdClaim, out int userId))
                {
                    return userId;
                }

                throw new UnauthorizedAccessException($"Invalid user ID format in token: {userIdClaim}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUserId: {ex.Message}");
                throw;
            }
        }
    }
}