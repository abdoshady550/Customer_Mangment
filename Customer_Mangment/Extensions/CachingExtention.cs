using Microsoft.AspNetCore.ResponseCompression;
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
            //Hybrid 
            services.AddHybridCache(option =>
            {
                option.DefaultEntryOptions = new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromMinutes(10),
                    LocalCacheExpiration = TimeSpan.FromSeconds(10)

                };
            });
            //output
            services.AddOutputCache(option =>
            {
                option.AddPolicy("customers", builder =>
                {
                    builder.SetVaryByQuery(["CustomerId"])
                           .SetVaryByHeader("X-Tenant-Id")
                           .Expire(TimeSpan.FromMinutes(10));
                    builder.Tag(["CustomerCache"]);
                });
            });
            //response
            services.AddResponseCaching();
            //response compression
            services.AddResponseCompression(option =>
            {
                option.EnableForHttps = true;
                option.Providers.Add<GzipCompressionProvider>();
                option.Providers.Add<BrotliCompressionProvider>();

                option.MimeTypes = new[]
                {
                    "application/json",
                    "text/plain",
                    "application/xml",
                };
            });
            services.Configure<GzipCompressionProviderOptions>(option =>
            {
                option.Level = System.IO.Compression.CompressionLevel.Fastest;
            });
            services.Configure<BrotliCompressionProviderOptions>(option =>
            {
                option.Level = System.IO.Compression.CompressionLevel.Fastest;
            });
            return services;
        }
    }


}
