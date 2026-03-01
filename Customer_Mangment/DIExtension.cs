using Customer_Mangment.Data;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Entities.History;
using Customer_Mangment.Repository;
using Customer_Mangment.Repository.Interfaces;
using Customer_Mangment.Repository.Services;
using Microsoft.EntityFrameworkCore;
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
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<ApplicationDbContextInitialiser>();


            services.AddScoped(typeof(IGenericRepo<>), typeof(GenericRepo<>));

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


            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<MongoDbInitialiser>();

            services.AddSingleton<IMongoClient>(_ => new MongoClient(settings.ConnectionString));

            services.AddSingleton(sp =>
                sp.GetRequiredService<IMongoClient>().GetDatabase(settings.DatabaseName));

            RegisterCollection<Customer>(services, "Customers");
            RegisterCollection<Address>(services, "Addresses");
            RegisterCollection<RefreshToken>(services, "RefreshTokens");
            RegisterCollection<User>(services, "Users");
            RegisterCollection<CustomerHistory>(services, "CustomerHistory");
            RegisterCollection<AddressHistory>(services, "AddressHistory");

            services.AddScoped(typeof(IGenericRepo<>), typeof(MongoGenericRepo<>));

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

                BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

                _serializersRegistered = true;
            }
        }
    }
}
