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
                throw new Exception("Имя пользователя или email уже существуют");

            var user = new User
            {
                Username = username.ToLower().Trim(),
                Email = email.ToLower().Trim(),
                FullName = fullName?.Trim() ?? "",
                Password = password, // Просто сохраняем пароль как есть
                Allergies = allergies ?? new List<string>(),
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);

            if (!await _userRepository.SaveAllAsync())
                throw new Exception("Не удалось сохранить пользователя");

            return user;
        }

        public async Task<User> Login(string username, string password)
        {
            var user = await _userRepository.GetUserByUsernameAsync(username.ToLower());

            if (user == null)
                throw new Exception("Пользователь не найден");

            if (user.Password != password)
                throw new Exception("Неверный пароль");

            return user;
        }
    }

    public interface IAuthService
    {
        Task<User> Register(string username, string email, string fullName, string password, List<string> allergies);
        Task<User> Login(string username, string password);
    }
}