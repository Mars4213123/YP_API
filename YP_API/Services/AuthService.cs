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

        public async Task<User> Login(string username, string password)
        {
            var user = await _userRepository.GetUserByUsernameAsync(username);

            if (user == null)
                throw new Exception("Пользователь не найден");

            if (user.Password != password)
                throw new Exception("Неверный пароль");

            return user;
        }

        public async Task<User> Register(string username, string email, string password)
        {
            var existingUser = await _userRepository.GetUserByUsernameAsync(username);
            if (existingUser != null)
                throw new Exception("Имя пользователя занято");

            var user = new User
            {
                Username = username,
                Email = email,
                Password = password
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveAllAsync();

            return user;
        }
    }

    public interface IAuthService
    {
        Task<User> Login(string username, string password);
        Task<User> Register(string username, string email, string password);
    }
}