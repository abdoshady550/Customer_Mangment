using Customer_Mangment.Repository.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Customer_Mangment.Repository.Services
{
    public sealed class MongoGenericRepo<T> : IGenericRepo<T> where T : class
    {
        private readonly IMongoCollection<T> _collection;

        private FilterDefinition<T> _filter = Builders<T>.Filter.Empty;
        private SortDefinition<T>? _sort;
        private int? _skip;
        private int? _take;

        public MongoGenericRepo(IMongoCollection<T> collection)
        {
            _collection = collection;

            if (HasSoftDelete())
            {
                var filter = Builders<T>.Filter.Eq("IsDeleted", false);
                _filter &= filter;
            }
        }


        public IGenericRepo<T> AsNoTracking() => this;

        public IGenericRepo<T> Include(Expression<Func<T, object>> include) => this;

        public IGenericRepo<T> Where(Expression<Func<T, bool>> predicate)
        {
            _filter &= Builders<T>.Filter.Where(predicate);
            return this;
        }

        public IGenericRepo<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            _sort = Builders<T>.Sort.Ascending(new ExpressionFieldDefinition<T, TKey>(keySelector));
            return this;
        }

        public IGenericRepo<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            _sort = Builders<T>.Sort.Descending(new ExpressionFieldDefinition<T, TKey>(keySelector));
            return this;
        }

        public IGenericRepo<T> OrderBy(Expression<Func<T, object>> keySelector)
        {
            _sort = Builders<T>.Sort.Ascending(keySelector);
            return this;
        }

        public IGenericRepo<T> OrderByDescending(Expression<Func<T, object>> keySelector)
        {
            _sort = Builders<T>.Sort.Descending(keySelector);
            return this;
        }

        public IGenericRepo<T> Skip(int count) { _skip = count; return this; }
        public IGenericRepo<T> Take(int count) { _take = count; return this; }


        public IGenericRepo<T> TemporalAll()
            => throw new NotSupportedException("Temporal queries require SQL Server.");

        public IGenericRepo<T> TemporalAsOf(DateTime dateTime)
            => throw new NotSupportedException("Temporal queries require SQL Server.");

        public IGenericRepo<T> TemporalBetween(DateTime from, DateTime to)
            => throw new NotSupportedException("Temporal queries require SQL Server.");

        public IGenericRepo<T> TemporalFromTo(DateTime from, DateTime to)
            => throw new NotSupportedException("Temporal queries require SQL Server.");


        public async Task<List<T>> ToListAsync(CancellationToken ct = default)
            => await BuildFluent().ToListAsync(ct);

        public async Task<T?> FirstOrDefaultAsync(CancellationToken ct = default)
            => await BuildFluent().FirstOrDefaultAsync(ct);

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        {
            var combinedFilter = _filter & Builders<T>.Filter.Where(predicate);
            return await _collection.Find(combinedFilter).FirstOrDefaultAsync(ct);
        }
        public Task<T?> FindAsync(int id, CancellationToken ct = default)
            => throw new NotSupportedException("FindAsync(int) is not supported for MongoDB. Use FirstOrDefaultAsync with a predicate.");


        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        {
            var combinedFilter = _filter & Builders<T>.Filter.Where(predicate);
            return await _collection.CountDocumentsAsync(combinedFilter, cancellationToken: ct) > 0;
        }

        public async Task<int> CountAsync(CancellationToken ct = default)
            => (int)await _collection.CountDocumentsAsync(_filter, cancellationToken: ct);

        public async Task<List<TResult>> SelectAsync<TResult>(Expression<Func<T, TResult>> selector, CancellationToken ct = default)
        {
            var docs = await ToListAsync(ct);
            return docs.Select(selector.Compile()).ToList();
        }

        public IQueryable<TResult> Select<TResult>(Expression<Func<T, TResult>> selector)
            => _collection.AsQueryable().Select(selector);

        public async Task AddAsync(T entity, CancellationToken ct = default)
            => await _collection.InsertOneAsync(entity, cancellationToken: ct);

        public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
            => await _collection.InsertManyAsync(entities, cancellationToken: ct);

        public void Update(T entity)
        {
            var id = GetId(entity);
            var filter = Builders<T>.Filter.Eq("_id", id);
            _collection.ReplaceOne(filter, entity);
        }

        public void Remove(T entity)
        {
            var id = GetId(entity);
            var filter = Builders<T>.Filter.Eq("_id", id);
            _collection.DeleteOne(filter);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            foreach (var entity in entities) Remove(entity);
        }

        public async Task<int> ExecuteDeleteAsync(CancellationToken ct = default)
        {
            var result = await _collection.DeleteManyAsync(_filter, ct);
            return (int)result.DeletedCount;
        }

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
            => Task.FromResult(0);

        public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
            => throw new NotSupportedException("Use IClientSessionHandle for MongoDB transactions.");


        private IFindFluent<T, T> BuildFluent()
        {
            var fluent = _collection.Find(_filter);
            if (_sort is not null) fluent = fluent.Sort(_sort);
            if (_skip.HasValue) fluent = fluent.Skip(_skip);
            if (_take.HasValue) fluent = fluent.Limit(_take);
            return fluent;
        }
        private static bool HasSoftDelete()
        {
            return typeof(T).GetProperty("IsDeleted") != null;
        }
        private static BsonBinaryData GetId(T entity)
        {
            var prop = typeof(T).GetProperty("Id")
                ?? throw new InvalidOperationException($"{typeof(T).Name} has no 'Id' property.");
            var value = prop.GetValue(entity)
                ?? throw new InvalidOperationException($"{typeof(T).Name}.Id is null.");
            var guid = (Guid)value;
            return new BsonBinaryData(guid, GuidRepresentation.Standard);
        }
    }

}
