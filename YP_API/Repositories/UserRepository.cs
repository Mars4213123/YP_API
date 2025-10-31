using Microsoft.EntityFrameworkCore;
using YP_API.Data;
using YP_API.Interfaces;
using YP_API.Models;

namespace YP_API.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(RecipePlannerContext context) : base(context) { }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.UserAllergies)
                    .ThenInclude(ua => ua.Allergy)
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<Allergy>> GetUserAllergiesAsync(int userId)
        {
            return await _context.UserAllergies
                .Where(ua => ua.UserId == userId)
                .Include(ua => ua.Allergy)
                .Select(ua => ua.Allergy)
                .ToListAsync();
        }

        public async Task<bool> UserExistsAsync(string username, string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Username == username || u.Email == email);
        }
    }
}
