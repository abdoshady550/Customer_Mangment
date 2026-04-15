
using Customer_Mangment.IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;


namespace Customer_Mangment.IdentityServer.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();
        await context.Database.EnsureCreatedAsync();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var appManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

        // Seed roles
        string[] roles = { "Admin", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Seed users
        await CreateUserIfNotExists(userManager, "admin@test.com", "Admin@123", "Admin", "Admin User");
        await CreateUserIfNotExists(userManager, "user@test.com", "User@123", "User", "Regular User");

        // Seed OpenIddict scopes
        await CreateScopeIfNotExists(scopeManager, Scopes.OpenId, "OpenID Connect", "Your user identifier");
        await CreateScopeIfNotExists(scopeManager, Scopes.Profile, "User profile", "Your user profile information");
        await CreateScopeIfNotExists(scopeManager, Scopes.Email, "Email address", "Your email address");
        await CreateScopeIfNotExists(scopeManager, Scopes.Roles, "User roles", "Your assigned roles");
        await CreateScopeIfNotExists(scopeManager, Scopes.OfflineAccess, "Offline access", "Request refresh tokens");
        await CreateScopeIfNotExists(scopeManager, "customer_api", "Customer API access", "Access to the Customer Management API");

        // Seed OpenIddict clients
        await CreateClientIfNotExists(appManager,
            clientId: "customer-management-swagger",
            displayName: "Swagger UI Client",
            clientSecret: null,
            grantTypes: [GrantTypes.Password, GrantTypes.RefreshToken],
            scopes: ["openid", "profile", "email", "roles", "offline_access", "customer_api"],
            redirectUris: [new Uri("http://localhost:5046/swagger/oauth2-redirect.html")],
            postLogoutRedirectUris: [new Uri("http://localhost:5046/swagger")]);

        await CreateClientIfNotExists(appManager,
            clientId: "customer-management-m2m",
            displayName: "Machine-to-Machine Client",
            clientSecret: "secret-for-m2m",
            grantTypes: [GrantTypes.ClientCredentials],
            scopes: ["customer_api"]);

        await SeedResourceServerAsync(appManager);

    }

    private static async Task CreateUserIfNotExists(
        UserManager<ApplicationUser> userManager,
        string email, string password, string role, string displayName)
    {
        if (await userManager.FindByEmailAsync(email) != null)
            return;

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            DisplayName = displayName

        };

        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }

    private static async Task CreateScopeIfNotExists(
        IOpenIddictScopeManager manager,
        string name,
        string displayName,
        string description)
    {
        if (await manager.FindByNameAsync(name) != null)
            return;

        await manager.CreateAsync(new OpenIddictScopeDescriptor
        {
            Name = name,
            DisplayName = displayName,
            Description = description,
            Resources = { "customer_api_resource" }
        });
    }
    private static async Task SeedResourceServerAsync(IOpenIddictApplicationManager manager)
    {
        if (await manager.FindByClientIdAsync("customer_api_resource") != null)
            return;

        await manager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = "customer_api_resource",
            ClientSecret = "api-secret",
            DisplayName = "Customer API Resource Server",
            Permissions =
        {
            OpenIddictConstants.Permissions.Endpoints.Introspection
        }
        });
    }
    private static async Task CreateClientIfNotExists(
        IOpenIddictApplicationManager manager,
        string clientId,
        string displayName,
        string? clientSecret,
        IEnumerable<string> grantTypes,
        IEnumerable<string> scopes,
        IEnumerable<Uri>? redirectUris = null,
        IEnumerable<Uri>? postLogoutRedirectUris = null)
    {
        if (await manager.FindByClientIdAsync(clientId) != null)
            return;

        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            DisplayName = displayName,
            Permissions =
            {
              OpenIddictConstants.Permissions.Endpoints.Token

            }
        };

        // Add grant types
        foreach (var grant in grantTypes)
        {
            descriptor.Permissions.Add(
                OpenIddictConstants.Permissions.Prefixes.GrantType + grant);
        }
        // Add scopes
        foreach (var scope in scopes)
            descriptor.Permissions.Add(OpenIddictConstants.Permissions.Prefixes.Scope + scope);

        // Client secret (if confidential)
        if (!string.IsNullOrEmpty(clientSecret))
            descriptor.ClientSecret = clientSecret;

        // Redirect URIs (for interactive clients)
        if (redirectUris != null)
        {
            foreach (var uri in redirectUris)
                descriptor.RedirectUris.Add(uri);
        }

        if (postLogoutRedirectUris != null)
        {
            foreach (var uri in postLogoutRedirectUris)
                descriptor.PostLogoutRedirectUris.Add(uri);
        }

        await manager.CreateAsync(descriptor);
    }
}