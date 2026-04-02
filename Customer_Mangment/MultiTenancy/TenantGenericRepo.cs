using Customer_Mangment.Data;
using Customer_Mangment.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace Customer_Mangment.MultiTenancy;

public sealed class TenantGenericRepo<T>(TenantDbContextFactory factory) : IGenericRepo<T>
    where T : class
{
    private AppDbContext Context => factory.Create();
    private IQueryable<T> Query => Context.Set<T>();

    //  builder state 

    private IQueryable<T> _query = null!;

    private IQueryable<T> CurrentQuery => _query ??= Context.Set<T>();

    private void ResetQuery() => _query = Context.Set<T>();

    //  fluent chain 

    public IGenericRepo<T> AsNoTracking()
    {
        _query = CurrentQuery.AsNoTracking();
        return this;
    }

    public IGenericRepo<T> Include(Expression<Func<T, object>> include)
    {
        _query = CurrentQuery.Include(include);
        return this;
    }

    public IGenericRepo<T> Where(Expression<Func<T, bool>> predicate)
    {
        _query = CurrentQuery.Where(predicate);
        return this;
    }

    public IGenericRepo<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
    {
        _query = CurrentQuery.OrderBy(keySelector);
        return this;
    }

    public IGenericRepo<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
    {
        _query = CurrentQuery.OrderByDescending(keySelector);
        return this;
    }

    public IGenericRepo<T> OrderBy(Expression<Func<T, object>> keySelector)
    {
        _query = CurrentQuery.OrderBy(keySelector);
        return this;
    }

    public IGenericRepo<T> OrderByDescending(Expression<Func<T, object>> keySelector)
    {
        _query = CurrentQuery.OrderByDescending(keySelector);
        return this;
    }

    public IGenericRepo<T> Skip(int count)
    {
        _query = CurrentQuery.Skip(count);
        return this;
    }

    public IGenericRepo<T> Take(int count)
    {
        _query = CurrentQuery.Take(count);
        return this;
    }

    public IGenericRepo<T> TemporalAll()
    {
        _query = Context.Set<T>().TemporalAll().IgnoreQueryFilters().AsNoTracking();
        return this;
    }

    public IGenericRepo<T> TemporalBetween(DateTime from, DateTime to)
    {
        _query = Context.Set<T>().TemporalBetween(from, to).IgnoreQueryFilters().AsNoTracking();
        return this;
    }

    public IGenericRepo<T> TemporalFromTo(DateTime from, DateTime to)
    {
        _query = Context.Set<T>().TemporalFromTo(from, to).IgnoreQueryFilters().AsNoTracking();
        return this;
    }

    public IGenericRepo<T> TemporalAsOf(DateTime dateTime)
    {
        _query = Context.Set<T>().TemporalAsOf(dateTime).IgnoreQueryFilters().AsNoTracking();
        return this;
    }

    //  operations 

    public async Task<List<T>> ToListAsync(CancellationToken ct = default)
    {
        var result = await CurrentQuery.ToListAsync(ct);
        ResetQuery();
        return result;
    }

    public async Task<T?> FirstOrDefaultAsync(CancellationToken ct = default)
    {
        var result = await CurrentQuery.FirstOrDefaultAsync(ct);
        ResetQuery();
        return result;
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        var result = await CurrentQuery.FirstOrDefaultAsync(predicate, ct);
        ResetQuery();
        return result;
    }

    public Task<T?> FindAsync(int id, CancellationToken ct = default)
        => Context.Set<T>().FindAsync([id], ct).AsTask()!;

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        var result = await CurrentQuery.AnyAsync(predicate, ct);
        ResetQuery();
        return result;
    }

    public async Task<int> CountAsync(CancellationToken ct = default)
    {
        var result = await CurrentQuery.CountAsync(ct);
        ResetQuery();
        return result;
    }

    public async Task<List<TResult>> SelectAsync<TResult>(
        Expression<Func<T, TResult>> selector,
        CancellationToken ct = default)
    {
        var result = await CurrentQuery.Select(selector).ToListAsync(ct);
        ResetQuery();
        return result;
    }

    public IQueryable<TResult> Select<TResult>(Expression<Func<T, TResult>> selector)
    {
        var result = CurrentQuery.Select(selector);
        ResetQuery();
        return result;
    }



    public async Task AddAsync(T entity, CancellationToken ct = default)
        => await Context.Set<T>().AddAsync(entity, ct);

    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
        => await Context.Set<T>().AddRangeAsync(entities, ct);

    public void Update(T entity)
        => Context.Set<T>().Update(entity);

    public void Remove(T entity)
        => Context.Set<T>().Remove(entity);

    public void RemoveRange(IEnumerable<T> entities)
        => Context.Set<T>().RemoveRange(entities);

    public async Task<int> ExecuteDeleteAsync(CancellationToken ct = default)
    {
        var result = await CurrentQuery.ExecuteDeleteAsync(ct);
        ResetQuery();
        return result;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await Context.SaveChangesAsync(ct);

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
        => await Context.Database.BeginTransactionAsync(ct);
}
