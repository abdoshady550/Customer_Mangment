using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace Customer_Mangment.Repository.Interfaces
{
    public interface IGenericRepo<T> where T : class
    {
        string? TenantId { get; }
        IGenericRepo<T> AsNoTracking();
        IGenericRepo<T> Include(Expression<Func<T, object>> include);
        IGenericRepo<T> Where(Expression<Func<T, bool>> predicate);
        IGenericRepo<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector);
        IGenericRepo<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector);
        IGenericRepo<T> Skip(int count);
        IGenericRepo<T> Take(int count);
        IGenericRepo<T> TemporalAll();

        IGenericRepo<T> TemporalAsOf(DateTime dateTime);

        IGenericRepo<T> TemporalBetween(DateTime from, DateTime to);

        IGenericRepo<T> TemporalFromTo(DateTime from, DateTime to);

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
        Task<List<TResult>> SelectAsync<TResult>(Expression<Func<T, TResult>> selector, CancellationToken ct = default);
        IGenericRepo<T> OrderByDescending(Expression<Func<T, object>> keySelector);
        IGenericRepo<T> OrderBy(Expression<Func<T, object>> keySelector);
        IQueryable<TResult> Select<TResult>(Expression<Func<T, TResult>> selector);
    }

}
