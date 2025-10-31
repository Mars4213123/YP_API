using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using YP_API.DTOs;
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

        public async Task<UserDto> Register(RegisterDto registerDto)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(registerDto.Username))
                throw new Exception("Username is required");

            if (string.IsNullOrWhiteSpace(registerDto.Email))
                throw new Exception("Email is required");

            if (string.IsNullOrWhiteSpace(registerDto.Password))
                throw new Exception("Password is required");

            if (await _userRepository.UserExistsAsync(registerDto.Username, registerDto.Email))
                throw new Exception("Username or email already exists");

            try
            {
                // Создаем соль и хеш
                using var hmac = new HMACSHA512();

                var passwordBytes = Encoding.UTF8.GetBytes(registerDto.Password);
                var passwordHash = hmac.ComputeHash(passwordBytes);
                var passwordSalt = hmac.Key;

                var user = new User
                {
                    Username = registerDto.Username.ToLower().Trim(),
                    Email = registerDto.Email.ToLower().Trim(),
                    FullName = registerDto.FullName?.Trim() ?? "",
                    Allergies = registerDto.Allergies ?? new List<string>(),
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    CreatedAt = DateTime.UtcNow
                };

                Console.WriteLine($"=== REGISTRATION DEBUG ===");
                Console.WriteLine($"Username: {user.Username}");
                Console.WriteLine($"PasswordHash length: {user.PasswordHash.Length}");
                Console.WriteLine($"PasswordSalt length: {user.PasswordSalt.Length}");
                Console.WriteLine($"==========================");

                await _userRepository.AddAsync(user);
                var result = await _userRepository.SaveAllAsync();

                if (!result)
                    throw new Exception("Failed to save user");

                Console.WriteLine($"User created successfully with ID: {user.Id}");

                return new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    Allergies = user.Allergies,
                    Token = CreateToken(user)
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Registration error: {ex}");
                throw new Exception($"Registration failed: {ex.Message}");
            }
        }

        public async Task<UserDto> Login(LoginDto loginDto)
        {
            var user = await _userRepository.GetUserByUsernameAsync(loginDto.Username.ToLower());

            if (user == null)
                throw new Exception("Invalid username");

            Console.WriteLine($"=== LOGIN DEBUG ===");
            Console.WriteLine($"Username: {user.Username}");
            Console.WriteLine($"Stored PasswordHash length: {user.PasswordHash?.Length ?? 0}");
            Console.WriteLine($"Stored PasswordSalt length: {user.PasswordSalt?.Length ?? 0}");
            Console.WriteLine($"===================");

            if (user.PasswordHash == null || user.PasswordSalt == null)
                throw new Exception("Invalid user data in database");

            // Вычисляем хеш введенного пароля
            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            // Сравниваем хеши
            if (computedHash.Length != user.PasswordHash.Length)
            {
                Console.WriteLine($"Hash length mismatch: computed={computedHash.Length}, stored={user.PasswordHash.Length}");
                throw new Exception("Invalid password");
            }

            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i])
                {
                    Console.WriteLine($"Hash mismatch at position {i}");
                    throw new Exception("Invalid password");
                }
            }

            Console.WriteLine("Password verification successful!");

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Allergies = user.Allergies,
                Token = CreateToken(user)
            };
        }

        public string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };

            // Используем HmacSha256 вместо HmacSha512
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }

    public interface IAuthService
    {
        Task<UserDto> Register(RegisterDto registerDto);
        Task<UserDto> Login(LoginDto loginDto);
        string CreateToken(User user);
    }
}
