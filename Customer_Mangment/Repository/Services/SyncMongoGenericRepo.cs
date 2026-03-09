using Customer_Mangment.Repository.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Customer_Mangment.Repository.Services
{
    public sealed class SyncMongoGenericRepo<T> : ISyncGenericRepo<T> where T : class
    {
        private readonly IMongoCollection<T> _collection;

        private FilterDefinition<T> _filter = Builders<T>.Filter.Empty;


        public SyncMongoGenericRepo(IMongoCollection<T> collection)
        {
            _collection = collection;

            if (HasSoftDelete())
            {
                var filter = Builders<T>.Filter.Eq("IsDeleted", false);
                _filter &= filter;
            }
        }

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

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
            => Task.FromResult(0);

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
