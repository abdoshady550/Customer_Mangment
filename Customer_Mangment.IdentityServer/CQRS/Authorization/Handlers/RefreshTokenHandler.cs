using Customer_Mangment.IdentityServer.CQRS.Authorization.Commands;
using Customer_Mangment.IdentityServer.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Collections.Immutable;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Customer_Mangment.IdentityServer.CQRS.Authorization.Handlers
{

    public class RefreshTokenHandler
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IOpenIddictScopeManager _scopeManager;

        public RefreshTokenHandler(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IOpenIddictScopeManager scopeManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _scopeManager = scopeManager;
        }

        public async Task<OpenIddictResponses> Handle(RefreshTokenGrantCommand command)
        {
            var httpContext = command.HttpContext;
            var auth = await httpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            var userId = auth.Principal?.FindFirst(Claims.Subject)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Forbidden();

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return Forbidden();

            var scopes = auth.Principal!.GetScopes();
            var principal = await CreatePrincipalAsync(user, scopes, httpContext);
            principal.SetClaim(Claims.Subject, user.Id);

            return new OpenIddictResponses
            {
                Principal = principal,
                AuthenticationScheme = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
            };
        }

        private async Task<ClaimsPrincipal> CreatePrincipalAsync(
            ApplicationUser user,
            ImmutableArray<string> scopes,
            HttpContext httpContext)
        {
            var principal = await _signInManager.CreateUserPrincipalAsync(user);
            var identity = (ClaimsIdentity)principal.Identity!;

            identity.SetClaim(Claims.Subject, user.Id)
                    .SetClaim(Claims.Email, user.Email)
                    .SetClaim(Claims.EmailVerified, user.EmailConfirmed)
                    .SetClaim(Claims.PhoneNumber, user.PhoneNumber)
                    .SetClaim(Claims.PhoneNumberVerified, user.PhoneNumberConfirmed)
                    .SetClaim("display_name", user.DisplayName ?? user.UserName);

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                if (Enum.TryParse<Role>(role, ignoreCase: true, out var roleEnum))
                    identity.AddClaim(Claims.Role, ((int)roleEnum).ToString());
                else
                    identity.AddClaim(Claims.Role, role);
            }

            if (httpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantId))
                identity.SetClaim("tenant_id", tenantId.ToString());

            identity.SetScopes(scopes);

            var resources = new List<string>();
            await foreach (var resource in _scopeManager.ListResourcesAsync(scopes))
                resources.Add(resource);
            identity.SetResources(resources);

            identity.SetDestinations(GetDestinations);
            return principal;
        }

        private static IEnumerable<string> GetDestinations(Claim claim) => claim.Type switch
        {
            Claims.Subject or Claims.Name or Claims.Email or "display_name" or "tenant_id"
                => new[] { Destinations.AccessToken, Destinations.IdentityToken },
            Claims.Role => new[] { Destinations.AccessToken },
            _ => new[] { Destinations.AccessToken }
        };

        private static OpenIddictResponses Forbidden() => new()
        {
            IsForbidden = true,
            AuthenticationScheme = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
        };
    }
}
