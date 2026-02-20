using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace Customer_Mangment.Repository.Interfaces
{
    public interface IGenericRepo<T> where T : class
    {
        IGenericRepo<T> AsNoTracking();
        IGenericRepo<T> Include(Expression<Func<T, object>> include);
        IGenericRepo<T> Where(Expression<Func<T, bool>> predicate);
        IGenericRepo<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector);
        IGenericRepo<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector);
        IGenericRepo<T> Skip(int count);
        IGenericRepo<T> Take(int count);

        Task<List<T>> ToListAsync(CancellationToken ct = default);
        Task<T?> FirstOrDefaultAsync(CancellationToken ct = default);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
        Task<T?> FindAsync(int id, CancellationToken ct = default);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
        Task<int> CountAsync(CancellationToken ct = default);

        Task AddAsync(T entity, CancellationToken ct = default);
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);
        void Update(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);

        Task<int> ExecuteDeleteAsync(CancellationToken ct = default);

        Task<int> SaveChangesAsync(CancellationToken ct = default);

        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
    }

}
