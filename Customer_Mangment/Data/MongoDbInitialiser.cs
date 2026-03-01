using Customer_Mangment.Model.Entities;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;

namespace Customer_Mangment.Data
{
    public class MongoDbInitialiser(
        ILogger<MongoDbInitialiser> logger,
        IMongoDatabase database)
    {
        private readonly ILogger<MongoDbInitialiser> _logger = logger;
        private readonly IMongoCollection<User> _users = database.GetCollection<User>("Users");
        private readonly IMongoCollection<IdentityRole> _roles = database.GetCollection<IdentityRole>("Roles");

        public async Task SeedAsync()
        {
            try
            {
                await TrySeedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding MongoDB.");
                throw;
            }
        }

        private async Task TrySeedAsync()
        {
            await SeedRolesAsync();
            await SeedUsersAsync();
        }

        private async Task SeedRolesAsync()
        {
            var roleNames = new[] { nameof(Role.Admin), nameof(Role.User) };

            foreach (var roleName in roleNames)
            {
                var exists = await _roles
                    .Find(r => r.Name == roleName)
                    .AnyAsync();

                if (!exists)
                {
                    await _roles.InsertOneAsync(new IdentityRole
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = roleName,
                        NormalizedName = roleName.ToUpperInvariant()
                    });

                    _logger.LogInformation("Seeded role: {Role}", roleName);
                }
            }
        }

        private async Task SeedUsersAsync()
        {
            var seeds = new[]
            {
                new
                {
                    Id    = "11111111-1111-1111-1111-111111111111",
                    Email = "admin@test.com",
                    Role  = nameof(Role.Admin)
                },
                new
                {
                    Id    = "22222222-2222-2222-2222-222222222222",
                    Email = "user@test.com",
                    Role  = nameof(Role.User)
                }
            };

            foreach (var seed in seeds)
            {
                var exists = await _users
                    .Find(u => u.Email == seed.Email)
                    .AnyAsync();

                if (!exists)
                {
                    var hasher = new PasswordHasher<User>();

                    var user = new User
                    {
                        Id = seed.Id,
                        UserName = seed.Email,
                        NormalizedUserName = seed.Email.ToUpperInvariant(),
                        Email = seed.Email,
                        NormalizedEmail = seed.Email.ToUpperInvariant(),
                        EmailConfirmed = true,
                    };

                    user.PasswordHash = hasher.HashPassword(user,
                        seed.Role == nameof(Role.Admin) ? "Admin@123" : "User@123");

                    await _users.InsertOneAsync(user);

                    _logger.LogInformation("Seeded user: {Email} with role: {Role}",
                        seed.Email, seed.Role);
                }
            }
        }
    }
}
