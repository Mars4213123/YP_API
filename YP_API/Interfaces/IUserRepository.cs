using YP_API.Models;

namespace YP_API.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetUserByUsernameAsync(string username);
        Task<User> GetUserByEmailAsync(string email);
        Task<bool> UserExistsAsync(string username, string email);
    }
}

