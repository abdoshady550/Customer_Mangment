using Customer_Mangment.Data;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Entities.History;
using Customer_Mangment.Repository;
using Customer_Mangment.Repository.Interfaces;
using Customer_Mangment.Repository.Interfaces.Audit;
using Customer_Mangment.Repository.Services;
using Customer_Mangment.Repository.Services.AuditServices;
using Customer_Mangment.Repository.Services.AuditServices.MongoDB;
using MassTransit;
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

            var mongoSettings = configuration
                .GetSection(MongoDbSettings.SectionName)
                .Get<MongoDbSettings>()
                ?? throw new InvalidOperationException(
                    $"Missing '{MongoDbSettings.SectionName}' section in appsettings.json.");

            RegisterMongoSerializers();

            services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoSettings.ConnectionString));
            services.AddSingleton(sp =>
                sp.GetRequiredService<IMongoClient>().GetDatabase(mongoSettings.DatabaseName));

            RegisterCollection<Customer>(services, "Customers");
            RegisterCollection<Address>(services, "Addresses");
            RegisterCollection<RefreshToken>(services, "RefreshTokens");
            RegisterCollection<User>(services, "Users");

            RegisterCollection<CustomerSnapshot>(services, "CustomerSnapshots");
            RegisterCollection<AddressSnapshot>(services, "AddressSnapshots");

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
            Console.WriteLine("Using SQL Server as Database Provider");

            services.AddScoped<ApplicationDbContextInitialiser>();

            services.AddScoped(typeof(IGenericRepo<>), typeof(GenericRepo<>));
            services.AddScoped<IHistoryService, SqlHistoryService>();
            services.AddScoped(typeof(ISyncGenericRepo<>), typeof(SyncMongoGenericRepo<>));

            return services;
        }

        // MongoDB

        private static IServiceCollection AddMongoDb(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            Console.WriteLine("Using MongoDB as Database Provider");
            services.AddScoped<MongoDbInitialiser>();

            RegisterCollection<Customer>(services, "Customers");
            RegisterCollection<Address>(services, "Addresses");
            RegisterCollection<RefreshToken>(services, "RefreshTokens");
            RegisterCollection<User>(services, "Users");

            RegisterCollection<CustomerSnapshot>(services, "CustomerSnapshots");
            RegisterCollection<AddressSnapshot>(services, "AddressSnapshots");


            services.AddScoped(typeof(IGenericRepo<>), typeof(MongoGenericRepo<>));
            services.AddScoped<IHistoryService, MongoHistoryService>();

            services.AddScoped<MongoSnapshotHandler>();

            //services.AddScoped(typeof(ISyncGenericRepo<>), typeof(SyncGenericRepo<>));


            return services;
        }

        public static IServiceCollection AddMassTransitWithRabbitMq(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<ISnapshotPublisher, SnapshotPublisher>();

            services.AddMassTransit(x =>
            {
                //  consumers
                x.AddConsumer<CustomerSnapshotConsumer>(cfg =>
                {
                    cfg.UseMessageRetry(r => r
                        .Exponential(
                            retryLimit: 5,
                            minInterval: TimeSpan.FromSeconds(2),
                            maxInterval: TimeSpan.FromMinutes(5),
                            intervalDelta: TimeSpan.FromSeconds(3)));

                    cfg.UseCircuitBreaker(cb =>
                    {
                        cb.TrackingPeriod = TimeSpan.FromMinutes(1);
                        cb.TripThreshold = 15;
                        cb.ActiveThreshold = 10;
                        cb.ResetInterval = TimeSpan.FromMinutes(5);
                    });
                });

                x.AddConsumer<AddressSnapshotConsumer>(cfg =>
                {
                    cfg.UseMessageRetry(r => r
                        .Exponential(
                            retryLimit: 5,
                            minInterval: TimeSpan.FromSeconds(2),
                            maxInterval: TimeSpan.FromMinutes(5),
                            intervalDelta: TimeSpan.FromSeconds(3)));

                    cfg.UseCircuitBreaker(cb =>
                    {
                        cb.TrackingPeriod = TimeSpan.FromMinutes(1);
                        cb.TripThreshold = 15;
                        cb.ActiveThreshold = 10;
                        cb.ResetInterval = TimeSpan.FromMinutes(5);
                    });
                });

                x.UsingRabbitMq((ctx, cfg) =>
                {
                    var rabbit = configuration.GetSection("RabbitMQ");

                    cfg.Host(rabbit["Host"] ?? "localhost", rabbit["VHost"] ?? "/", h =>
                    {
                        h.Username(rabbit["Username"] ?? "admin");
                        h.Password(rabbit["Password"] ?? "admin123");
                    });

                    cfg.ReceiveEndpoint("customer-snapshots", e =>
                    {
                        e.Durable = true;
                        e.AutoDelete = false;
                        e.ConfigureConsumer<CustomerSnapshotConsumer>(ctx);
                        e.BindDeadLetterQueue("customer-snapshots_dead-letter", "customer-snapshots_error");

                    });

                    cfg.ReceiveEndpoint("address-snapshots", e =>
                    {
                        e.Durable = true;
                        e.AutoDelete = false;
                        e.ConfigureConsumer<AddressSnapshotConsumer>(ctx);

                        e.BindDeadLetterQueue("address-snapshots_dead-letter", "address-snapshots_error");
                    });

                    cfg.ConfigureEndpoints(ctx);
                });
            });

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
