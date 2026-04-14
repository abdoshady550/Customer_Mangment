using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Customer_Mangment.Data;

public sealed class OpenIddictDataSeeder(IConfiguration configuration,
                                         IOpenIddictApplicationManager applicationManager,
                                         IOpenIddictScopeManager scopeManager)
{
    private readonly IConfiguration _configuration = configuration;
    private readonly IOpenIddictApplicationManager _applicationManager = applicationManager;
    private readonly IOpenIddictScopeManager _scopeManager = scopeManager;

    public async Task SeedAsync(CancellationToken ct = default)
    {
        var apiResourceName = _configuration["OpenIddict:ApiResourceName"] ?? "customer_management_api";
        var publicClientId = _configuration["OpenIddict:PublicClientId"] ?? "customer-management-swagger";

        if (await _scopeManager.FindByNameAsync("api", ct) is null)
        {
            await _scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "api",
                DisplayName = "Customer Management API",
                Resources = { apiResourceName }
            }, ct);
        }

        if (await _scopeManager.FindByNameAsync("roles", ct) is null)
        {
            await _scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "roles",
                DisplayName = "Access user roles",
                Resources = { apiResourceName }
            }, ct);
        }

        if (await _applicationManager.FindByClientIdAsync(publicClientId, ct) is null)
        {
            var descriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = publicClientId,
                ClientType = ClientTypes.Public,
                ConsentType = ConsentTypes.Implicit,
                DisplayName = "Customer Management Local Client"
            };

            descriptor.Permissions.UnionWith(
            [
                Permissions.Endpoints.Token,
                Permissions.GrantTypes.Password,
                Permissions.GrantTypes.RefreshToken,
                Permissions.Scopes.Email,
                Permissions.Scopes.Profile,
                Permissions.Prefixes.Scope + Scopes.OfflineAccess,
                Permissions.Prefixes.Scope + "api",
                Permissions.Prefixes.Scope + "roles"
            ]);

            await _applicationManager.CreateAsync(descriptor, ct);
        }
    }
}
