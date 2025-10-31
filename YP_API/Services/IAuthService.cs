using YP_API.DTOs;
using YP_API.Models;

namespace YP_API.Services
{
    public interface IAuthService
    {
        Task<UserDto> Register(RegisterDto registerDto);
        Task<UserDto> Login(LoginDto loginDto);
        string CreateToken(User user);
    }
}
