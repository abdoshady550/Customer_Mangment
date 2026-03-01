using System.Linq.Expressions;

namespace Customer_Mangment.Repository.Interfaces
{
    public interface IMongoGenericRepo<T> where T : class
    {
        IMongoGenericRepo<T> Where(Expression<Func<T, bool>> predicate);
        IMongoGenericRepo<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector);
        IMongoGenericRepo<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector);
        IMongoGenericRepo<T> Skip(int count);
        IMongoGenericRepo<T> Take(int count);

        Task<List<T>> ToListAsync(CancellationToken ct = default);
        Task<T?> FirstOrDefaultAsync(CancellationToken ct = default);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
        Task<int> CountAsync(CancellationToken ct = default);

        Task AddAsync(T entity, CancellationToken ct = default);
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);
        Task UpdateAsync(T entity, Expression<Func<T, bool>> filter, CancellationToken ct = default);
        Task RemoveAsync(Expression<Func<T, bool>> filter, CancellationToken ct = default);
    }
}
