using Customer_Mangment.Model.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Customer_Mangment.Data
{
    public class ApplicationDbContextInitialiser(
        ILogger<ApplicationDbContextInitialiser> logger,
        AppDbContext context,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        private readonly ILogger<ApplicationDbContextInitialiser> _logger = logger;
        private readonly AppDbContext _context = context;
        private readonly UserManager<User> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;

        public async Task InitialiseAsync()
        {
            try
            {
                await _context.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initialising the database.");
                throw;
            }
        }

        public async Task SeedAsync()
        {
            try
            {
                await TrySeedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }

        public async Task TrySeedAsync()
        {
            // ===== Roles =====
            var adminRole = new IdentityRole(nameof(Role.Admin));
            if (!_roleManager.Roles.Any(r => r.Name == adminRole.Name))
            {
                await _roleManager.CreateAsync(adminRole);
            }

            var userRole = new IdentityRole(nameof(Role.User));
            if (!_roleManager.Roles.Any(r => r.Name == userRole.Name))
            {
                await _roleManager.CreateAsync(userRole);
            }

            // ===== Admin User =====
            var adminEmail = "admin@test.com";

            if (await _userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new User
                {
                    Id = "11111111-1111-1111-1111-111111111111",
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                await _userManager.CreateAsync(admin, "Admin@123");
                await _userManager.AddToRoleAsync(admin, "Admin");
            }

            // ===== Normal User =====
            var userEmail = "user@test.com";

            if (await _userManager.FindByEmailAsync(userEmail) == null)
            {
                var user = new User
                {
                    Id = "22222222-2222-2222-2222-222222222222",
                    UserName = userEmail,
                    Email = userEmail,
                    EmailConfirmed = true
                };

                await _userManager.CreateAsync(user, "User@123");
                await _userManager.AddToRoleAsync(user, "User");
            }
        }
    }

    public static class InitialiserExtension
    {
        public static async Task InitialiseDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var flags = scope.ServiceProvider
                .GetRequiredService<IConfiguration>()
                .GetSection(FeatureFlags.SectionName)
                .Get<FeatureFlags>() ?? new FeatureFlags();

            if (flags.UseMongoDb)
            {
                var mongoInitialiser = scope.ServiceProvider
                    .GetRequiredService<MongoDbInitialiser>();

                await mongoInitialiser.SeedAsync();
            }
            else
            {
                var sqlInitialiser = scope.ServiceProvider
                    .GetRequiredService<ApplicationDbContextInitialiser>();
                var openIddictSeeder = scope.ServiceProvider
                    .GetRequiredService<OpenIddictDataSeeder>();

                await sqlInitialiser.InitialiseAsync();
                await sqlInitialiser.SeedAsync();
                await openIddictSeeder.SeedAsync();
            }
        }
    }

}
