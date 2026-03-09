namespace Customer_Mangment.Repository.Interfaces
{
    public interface ISyncGenericRepo<T> where T : class
    {
        Task AddAsync(T entity, CancellationToken ct = default);
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);
        void Update(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);

        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }

}
