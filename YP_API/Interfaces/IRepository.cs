using Microsoft.EntityFrameworkCore;
using YP_API.Data;
using YP_API.Helpers;
using YP_API.Models;
using YP_API.Repositories;

namespace YP_API.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task<bool> SaveAllAsync();
    }
}

