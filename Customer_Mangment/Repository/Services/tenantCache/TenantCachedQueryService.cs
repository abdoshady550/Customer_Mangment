using Customer_Mangment.Data;
using Customer_Mangment.MultiTenancy;
using Customer_Mangment.Repository.Interfaces.tenantCache;
using Microsoft.Extensions.Caching.Hybrid;

namespace Customer_Mangment.Repository.Services.tenantCache
{
    internal sealed class TenantCachedQueryService(HybridCache cache,
                                                   IServiceProvider serviceProvider,
                                                   ITenantContext tenantContext) : ITenantCachedQueryService
    {
        public Boolean LoadedFromDb { get; set; } = false;


        public async Task<List<TResult>> GetOrCreateAsync<TResult>(
            string cacheKeyPrefix,
            Func<AppDbContext, CancellationToken, Task<List<TResult>>> queryFactory,
            CancellationToken ct)
        {
            var tenantId = tenantContext.TenantId;
            var connectionString = tenantContext.ConnectionString;
            var cacheKey = $"{cacheKeyPrefix}_{tenantId}";

            var result = await cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                LoadedFromDb = true;
                await using var scope = serviceProvider.CreateAsyncScope();
                var dbFactory = scope.ServiceProvider.GetRequiredService<TenantDbContextFactory>();
                await using var dbContext = dbFactory.Create(tenantId, connectionString);

                return await queryFactory(dbContext, ct);
            }, cancellationToken: ct);

            return result ?? new List<TResult>();
        }
    }
}
