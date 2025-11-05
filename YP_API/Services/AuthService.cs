using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using YP_API.Interfaces;
using YP_API.Models;

namespace YP_API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _config;

        public AuthService(IUserRepository userRepository, IConfiguration config)
        {
            _userRepository = userRepository;
            _config = config;
        }

        public async Task<User> Register(string username, string email, string fullName, string password, List<string> allergies)
        {
            if (await _userRepository.UserExistsAsync(username, email))
                throw new Exception("Username or email already exists");

            using var hmac = new HMACSHA512();

            var user = new User
            {
                Username = username.ToLower().Trim(),
                Email = email.ToLower().Trim(),
                FullName = fullName?.Trim() ?? "",
                Allergies = allergies ?? new List<string>(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password)),
                PasswordSalt = hmac.Key,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);

            if (!await _userRepository.SaveAllAsync())
                throw new Exception("Failed to save user");

            user.Token = CreateToken(user);
            return user;
        }

        public async Task<User> Login(string username, string password)
        {
            var user = await _userRepository.GetUserByUsernameAsync(username.ToLower());

            if (user == null)
                throw new Exception("Invalid username");

            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i])
                    throw new Exception("Invalid password");
            }

            user.Token = CreateToken(user);
            return user;
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("userId", user.Id.ToString()),
                new Claim("email", user.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = creds,
                Issuer = "RecipePlannerAPI",
                Audience = "RecipePlannerUsers"
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }

    public interface IAuthService
    {
        Task<User> Register(string username, string email, string fullName, string password, List<string> allergies);
        Task<User> Login(string username, string password);
    }
}