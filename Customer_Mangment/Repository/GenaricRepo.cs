using Customer_Mangment.Data;
using Customer_Mangment.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace Customer_Mangment.Repository
{
    public class GenericRepo<T>(AppDbContext context) : IGenericRepo<T> where T : class
    {
        private readonly AppDbContext _context = context;
        private IQueryable<T> _query = context.Set<T>();

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
        public IGenericRepo<T> TemporalAll()
        {
            _query = _context.Set<T>()
                .TemporalAll()
                .IgnoreQueryFilters()
                .AsNoTracking();

            return this;
        }
        public IGenericRepo<T> TemporalBetween(DateTime from, DateTime to)
        {
            _query = _context.Set<T>()
                .TemporalBetween(from, to)
                .IgnoreQueryFilters()
                .AsNoTracking();

            return this;
        }

        public IGenericRepo<T> TemporalFromTo(DateTime from, DateTime to)
        {
            _query = _context.Set<T>()
                .TemporalFromTo(from, to)
                .IgnoreQueryFilters()
                .AsNoTracking();

            return this;
        }

        public IGenericRepo<T> TemporalAsOf(DateTime dateTime)
        {
            _query = _context.Set<T>()
                .TemporalAsOf(dateTime)
                .IgnoreQueryFilters()
                .AsNoTracking();

            return this;
        }
        public IGenericRepo<T> OrderByDescending(Expression<Func<T, object>> keySelector)
        {
            _query = _query.OrderByDescending(keySelector);

            return this;
        }
        public IGenericRepo<T> OrderBy(Expression<Func<T, object>> keySelector)
        {
            _query = _query.OrderBy(keySelector);

            return this;
        }


        public async Task<List<TResult>> SelectAsync<TResult>(
            Expression<Func<T, TResult>> selector,
            CancellationToken ct = default)
        {
            return await _query
                .Select(selector)
                .ToListAsync(ct);

        }

        public IQueryable<TResult> Select<TResult>(Expression<Func<T, TResult>> selector)
        {
            var result = _query.Select(selector);
            Reset();
            return result;
        }
        public async Task<List<T>> ToListAsync(CancellationToken ct = default)
        {
            var result = await _query.ToListAsync(ct);
            Reset();
            return result;
        }

        public async Task<T?> FirstOrDefaultAsync(CancellationToken ct = default)
        {
            var result = await _query.FirstOrDefaultAsync(ct);
            Reset();
            return result;
        }

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        {
            var result = await _query.FirstOrDefaultAsync(predicate, ct);
            Reset();
            return result;
        }

        public async Task<T?> FindAsync(int id, CancellationToken ct = default)
            => await _context.Set<T>().FindAsync(id, ct);

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        {
            var result = await _query.AnyAsync(predicate, ct);
            Reset();
            return result;
        }
        public async Task<int> CountAsync(CancellationToken ct = default)
        {
            var result = await _query.CountAsync(ct);
            Reset();
            return result;
        }

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
        public async Task<int> ExecuteDeleteAsync(CancellationToken ct = default)
            => await _query.ExecuteDeleteAsync(ct);


        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
            => await _context.SaveChangesAsync(ct);

        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
            => await _context.Database.BeginTransactionAsync(ct);


        private void Reset()
        {
            _query = _context.Set<T>();
        }
    }
}