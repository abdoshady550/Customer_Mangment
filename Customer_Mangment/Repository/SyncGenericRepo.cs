using Customer_Mangment.Data;
using Customer_Mangment.Repository.Interfaces;

namespace Customer_Mangment.Repository
{
    public class SyncGenericRepo<T>(AppDbContext context) : ISyncGenericRepo<T> where T : class
    {
        private readonly AppDbContext _context = context;

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