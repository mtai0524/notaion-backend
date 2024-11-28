using System.Linq.Expressions;

namespace Notaion.Domain.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        // get
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> predicate);
        Task<T> GetByIdAsync(int id);

        // add 
        Task<T> AddAsync(T entity);

        // modifier
        Task UpdateAsync(T entity);
        Task UpdateRangeAsync(IEnumerable<T> entities);

        // delete
        Task DeleteAsync(string id);

        // paging 
        Task<IEnumerable<T>> GetPaginatedAsync(Expression<Func<T, bool>> predicate, int pageNumber, int pageSize);

        Task<int> CountAsync(Expression<Func<T, bool>> predicate);
    }
}
