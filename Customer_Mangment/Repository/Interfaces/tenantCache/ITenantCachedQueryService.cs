using Customer_Mangment.Data;

namespace Customer_Mangment.Repository.Interfaces.tenantCache
{
    public interface ITenantCachedQueryService
    {
        public Boolean LoadedFromDb { get; set; }

        Task<List<TResult>> GetOrCreateAsync<TResult>(
            string cacheKeyPrefix,
            Func<AppDbContext, CancellationToken, Task<List<TResult>>> queryFactory,
            CancellationToken ct);
    }

}
