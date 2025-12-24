using Microsoft.EntityFrameworkCore;
using YP_API.Data;
using YP_API.Interfaces;
using YP_API.Models;

namespace YP_API.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly RecipePlannerContext _context;

        public UserRepository(RecipePlannerContext context)
        {
            _context = context;
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task AddAsync(User entity)
        {
            await _context.Users.AddAsync(entity);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}