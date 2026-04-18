using Customer_Mangment.IdentityServer.CQRS.Authorization.Commands;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Customer_Mangment.IdentityServer.CQRS.Authorization.Handlers
{
    public class ClientCredentialsTokenHandler
    {
        private readonly IOpenIddictApplicationManager _applicationManager;
        private readonly IOpenIddictScopeManager _scopeManager;

        public ClientCredentialsTokenHandler(
            IOpenIddictApplicationManager applicationManager,
            IOpenIddictScopeManager scopeManager)
        {
            _applicationManager = applicationManager;
            _scopeManager = scopeManager;
        }

        public async Task<OpenIddictResponses> Handle(ClientCredentialsTokenCommand command)
        {
            var request = command.Request;
            var application = await _applicationManager.FindByClientIdAsync(request.ClientId!);
            if (application is null)
                return Forbidden();

            var identity = new ClaimsIdentity(
                TokenValidationParameters.DefaultAuthenticationType,
                Claims.Name,
                Claims.Role);

            var clientId = await _applicationManager.GetClientIdAsync(application);
            var displayName = await _applicationManager.GetDisplayNameAsync(application);

            identity.SetClaim(Claims.Subject, clientId)
                    .SetClaim(Claims.Name, displayName);

            var scopes = request.GetScopes();
            identity.SetScopes(scopes);

            var resources = new List<string>();
            await foreach (var resource in _scopeManager.ListResourcesAsync(scopes))
                resources.Add(resource);
            identity.SetResources(resources);

            // ✅ Required so OpenIddict knows which claims to include in the token
            identity.SetDestinations(claim => claim.Type switch
            {
                Claims.Subject or Claims.Name
                    => new[] { Destinations.AccessToken },
                _ => new[] { Destinations.AccessToken }
            });

            var principal = new ClaimsPrincipal(identity);
            principal.SetClaim(Claims.ClientId, clientId);

            return new OpenIddictResponses
            {
                Principal = principal,
                AuthenticationScheme = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
            };
        }

        private static OpenIddictResponses Forbidden() => new()
        {
            IsForbidden = true,
            AuthenticationScheme = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
        };
    }
}
