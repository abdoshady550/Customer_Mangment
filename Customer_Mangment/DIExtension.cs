using Customer_Mangment.Data;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Entities.History;
using Customer_Mangment.Repository;
using Customer_Mangment.Repository.Interfaces;
using Customer_Mangment.Repository.Interfaces.Audit;
using Customer_Mangment.Repository.Services;
using Customer_Mangment.Repository.Services.AuditServices;
using Customer_Mangment.Repository.Services.AuditServices.MongoDB;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Customer_Mangment
{
    public static class DIExtension
    {
        public static IServiceCollection AddDataBaseConfig(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var flags = configuration
                .GetSection(FeatureFlags.SectionName)
                .Get<FeatureFlags>() ?? new FeatureFlags();

            if (flags.UseMongoDb)
            {
                services.AddMongoDb(configuration);
            }
            else
            {
                services.AddSqlServer(configuration);
            }

            return services;
        }

        // SQL
        private static IServiceCollection AddSqlServer(
            this IServiceCollection services,
            IConfiguration configuration)
        {

            services.AddScoped<ApplicationDbContextInitialiser>();

            services.AddScoped(typeof(IGenericRepo<>), typeof(GenericRepo<>));
            services.AddScoped<ISnapshotService, SqlSnapshotService>();
            services.AddScoped<IHistoryService, SqlHistoryService>();

            return services;
        }

        // MongoDB

        private static IServiceCollection AddMongoDb(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var settings = configuration
                .GetSection(MongoDbSettings.SectionName)
                .Get<MongoDbSettings>()
                ?? throw new InvalidOperationException(
                    $"Missing '{MongoDbSettings.SectionName}' section in appsettings.json.");

            RegisterMongoSerializers();

            services.AddScoped<MongoDbInitialiser>();

            services.AddSingleton<IMongoClient>(_ => new MongoClient(settings.ConnectionString));

            services.AddSingleton(sp =>
                sp.GetRequiredService<IMongoClient>().GetDatabase(settings.DatabaseName));

            RegisterCollection<Customer>(services, "Customers");
            RegisterCollection<Address>(services, "Addresses");
            RegisterCollection<RefreshToken>(services, "RefreshTokens");
            RegisterCollection<User>(services, "Users");

            RegisterCollection<CustomerSnapshot>(services, "CustomerSnapshots");
            RegisterCollection<AddressSnapshot>(services, "AddressSnapshots");


            services.AddScoped(typeof(IGenericRepo<>), typeof(MongoGenericRepo<>));
            services.AddScoped<ISnapshotService, MongoSnapshotService>();
            services.AddScoped<IHistoryService, MongoHistoryService>();

            return services;
        }

        private static void RegisterCollection<T>(IServiceCollection services, string collectionName) where T : class
        {
            services.AddSingleton(sp =>
                sp.GetRequiredService<IMongoDatabase>().GetCollection<T>(collectionName));
        }
        private static bool _serializersRegistered = false;
        private static readonly object _lock = new();

        private static void RegisterMongoSerializers()
        {
            lock (_lock)
            {
                if (_serializersRegistered) return;

                var guidSerializer = new GuidSerializer(GuidRepresentation.Standard);

                BsonClassMap.RegisterClassMap<Customer>(cm =>
                {
                    cm.AutoMap();
                    cm.MapIdProperty(c => c.Id).SetSerializer(guidSerializer);
                });

                BsonClassMap.RegisterClassMap<Address>(cm =>
                {
                    cm.AutoMap();
                    cm.MapIdProperty(a => a.Id).SetSerializer(guidSerializer);
                    cm.MapProperty(a => a.CustomerId).SetSerializer(guidSerializer);
                });

                BsonClassMap.RegisterClassMap<RefreshToken>(cm =>
                {
                    cm.AutoMap();
                    cm.MapIdProperty(r => r.Id).SetSerializer(guidSerializer);
                });

                _serializersRegistered = true;
            }
        }
    }
}
