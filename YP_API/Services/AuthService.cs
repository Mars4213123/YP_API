using System.Security.Cryptography;
using System.Text;
using YP_API.Interfaces;
using YP_API.Models;

namespace YP_API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
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

            return user;
        }
    }

    public interface IAuthService
    {
        Task<User> Register(string username, string email, string fullName, string password, List<string> allergies);
        Task<User> Login(string username, string password);
    }
}