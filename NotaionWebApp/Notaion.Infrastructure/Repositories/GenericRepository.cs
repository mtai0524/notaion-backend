using Microsoft.EntityFrameworkCore;
using Notaion.Domain.Interfaces;
using Notaion.Infrastructure.Context;
using System.Linq.Expressions;

namespace Notaion.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        public readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;
        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }
        public async Task<T> AddAsync(T entity)
        {
            var entityEntry = await _dbSet.AddAsync(entity);
            return entityEntry.Entity;
        }
        public async Task<T> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

        public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync(); // _context.Chat.ToListAsync()

        public async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet
                .Where(predicate)
                .OrderByDescending(x => EF.Property<DateTime>(x, "SentDate"))
                .ToListAsync();
        }
        public async Task UpdateAsync(T entity) => _dbSet.Update(entity);

        public async Task UpdateRangeAsync(IEnumerable<T> entities)
        {
            _dbSet.AttachRange(entities);
            foreach (var entity in entities)
            {
                _context.Entry(entity).State = EntityState.Modified;
            }
            await Task.CompletedTask;
        }
        public async Task DeleteAsync(string id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
                _dbSet.Remove(entity);
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet
                .Where(predicate)
                .CountAsync();
        }

        public async Task<IEnumerable<T>> GetPaginatedAsync(Expression<Func<T, bool>> predicate, int pageNumber, int pageSize)
        {
            return await _dbSet
                .Where(predicate)
                .OrderByDescending(x => EF.Property<DateTime>(x, "SentDate"))  // Thêm sắp xếp theo ngày gửi (nếu cần)
                .Skip((pageNumber - 1) * pageSize)  // Bỏ qua các bản ghi trước trang hiện tại
                .Take(pageSize)  // Lấy số lượng bản ghi trong trang
                .ToListAsync();
        }
    }
}
