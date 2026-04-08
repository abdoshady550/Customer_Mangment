using Microsoft.Extensions.Caching.Hybrid;

namespace Customer_Mangment.Extensions
{
    public static class CachingExtention
    {
        public static IServiceCollection AddCaching(this IServiceCollection services, IConfiguration configuration)
        {
            //InMemory
            services.AddMemoryCache(option => option.SizeLimit = 100);
            //Distributed redis
            services.AddStackExchangeRedisCache(option =>
            {
                option.Configuration = configuration.GetConnectionString("redis");
                option.InstanceName = "CustomerManagement_Api:";

            });
            //Distributed sql
            services.AddDistributedSqlServerCache(option =>
            {
                option.ConnectionString = configuration.GetConnectionString("sqlServerCache");
                option.SchemaName = "dbo";
                option.TableName = "CacheEntries";

            });
            services.AddHybridCache(option =>
            {
                option.DefaultEntryOptions = new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromMinutes(10),
                    LocalCacheExpiration = TimeSpan.FromSeconds(10)

                };
            });
            return services;
        }
    }


}
