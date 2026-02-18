using Customer_Mangment.Data;
using Customer_Mangment.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Customer_Mangment.Repository
{
    public class GenericRepo<T> : IGenericRepo<T> where T : class
    {
        private readonly AppDbContext _context;
        private IQueryable<T> _query;

        public GenericRepo(AppDbContext context)
        {
            _context = context;
            _query = context.Set<T>();
        }

        public IGenericRepo<T> AsNoTracking()
        {
            _query = _query.AsNoTracking();

            return this;
        }

        public IGenericRepo<T> Include(Expression<Func<T, object>> include)
        {
            _query = _query.Include(include);
            return this;
        }

        public IGenericRepo<T> Where(Expression<Func<T, bool>> predicate)
        {
            _query = _query.Where(predicate);
            return this;
        }

        public IGenericRepo<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            _query = _query.OrderBy(keySelector);
            return this;
        }

        public IGenericRepo<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            _query = _query.OrderByDescending(keySelector);
            return this;
        }

        public IGenericRepo<T> Skip(int count)
        {
            _query = _query.Skip(count);
            return this;
        }

        public IGenericRepo<T> Take(int count)
        {
            _query = _query.Take(count);
            return this;
        }

        public async Task<List<T>> ToListAsync(CancellationToken ct = default)
            => await _query.ToListAsync(ct);

        public async Task<T?> FirstOrDefaultAsync(CancellationToken ct = default)
            => await _query.FirstOrDefaultAsync(ct);

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
            => await _query.FirstOrDefaultAsync(predicate, ct);

        public async Task<T?> FindAsync(int id, CancellationToken ct = default)
            => await _context.Set<T>().FindAsync(id, ct);

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
            => await _query.AnyAsync(predicate, ct);

        public async Task<int> CountAsync(CancellationToken ct = default)
            => await _query.CountAsync(ct);


        public async Task AddAsync(T entity, CancellationToken ct = default)
            => await _context.Set<T>().AddAsync(entity, ct);

        public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
            => await _context.Set<T>().AddRangeAsync(entities, ct);

        public void Update(T entity)
            => _context.Set<T>().Update(entity);

        public void Remove(T entity)
            => _context.Set<T>().Remove(entity);

        public void RemoveRange(IEnumerable<T> entities)
            => _context.Set<T>().RemoveRange(entities);


        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
            => await _context.SaveChangesAsync(ct);
    }
}