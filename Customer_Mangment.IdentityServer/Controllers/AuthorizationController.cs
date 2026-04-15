using Customer_Mangment.IdentityServer.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Customer_Mangment.IdentityServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IOpenIddictApplicationManager _applicationManager;
        private readonly IOpenIddictScopeManager _scopeManager;

        public AuthorizationController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IOpenIddictApplicationManager applicationManager,
            IOpenIddictScopeManager scopeManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _applicationManager = applicationManager;
            _scopeManager = scopeManager;
        }

        [HttpPost("~/connect/token")]
        public async Task<IActionResult> Exchange()
        {
            var request = HttpContext.GetOpenIddictServerRequest();
            if (request == null)
                return BadRequest("Invalid OpenIddict request");

            if (request.IsPasswordGrantType())
                return await HandlePasswordGrant(request);

            if (request.IsClientCredentialsGrantType())
                return await HandleClientCredentialsGrant(request);

            if (request.IsRefreshTokenGrantType())
                return await HandleRefreshTokenGrant(request);

            return BadRequest("Unsupported grant type");
        }

        // PASSWORD GRANT
        private async Task<IActionResult> HandlePasswordGrant(OpenIddictRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Username);
            if (user == null)
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var result = await _signInManager.CheckPasswordSignInAsync(
                user,
                request.Password,
                lockoutOnFailure: false);

            if (!result.Succeeded)
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var principal = await CreateClaimsPrincipalAsync(user, request.GetScopes());

            principal.SetClaim(Claims.Subject, user.Id);

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        //  CLIENT CREDENTIALS 
        private async Task<IActionResult> HandleClientCredentialsGrant(OpenIddictRequest request)
        {
            var application = await _applicationManager.FindByClientIdAsync(request.ClientId);
            if (application == null)
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var identity = new ClaimsIdentity(
                TokenValidationParameters.DefaultAuthenticationType);

            var clientId = await _applicationManager.GetClientIdAsync(application);
            var displayName = await _applicationManager.GetDisplayNameAsync(application);

            identity.SetClaim(Claims.Subject, clientId);
            identity.SetClaim(Claims.Name, displayName);

            identity.SetScopes(request.GetScopes());

            var resources = new List<string>();
            await foreach (var resource in _scopeManager.ListResourcesAsync(identity.GetScopes()))
                resources.Add(resource);

            identity.SetResources(resources);

            var principal = new ClaimsPrincipal(identity);
            principal.SetClaim(Claims.ClientId, clientId);

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        //  REFRESH TOKEN 
        private async Task<IActionResult> HandleRefreshTokenGrant(OpenIddictRequest request)
        {
            var info = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var userId = info.Principal?.FindFirst(Claims.Subject)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var principal = await CreateClaimsPrincipalAsync(user, info.Principal.GetScopes());

            principal.SetClaim(Claims.Subject, user.Id);

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        //  CORE FACTORY 
        private async Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(
            ApplicationUser user,
            IEnumerable<string> scopes)
        {
            var principal = await _signInManager.CreateUserPrincipalAsync(user);
            var identity = (ClaimsIdentity)principal.Identity!;

            identity.SetClaim(Claims.Subject, user.Id);
            identity.SetClaim(Claims.Email, user.Email);
            identity.SetClaim(Claims.EmailVerified, user.EmailConfirmed);
            identity.SetClaim(Claims.PhoneNumber, user.PhoneNumber);
            identity.SetClaim(Claims.PhoneNumberVerified, user.PhoneNumberConfirmed);
            identity.SetClaim("display_name", user.DisplayName ?? user.UserName);

            // Roles
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
                identity.AddClaim(Claims.Role, role);

            // Tenant
            if (HttpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantId))
                identity.SetClaim("tenant_id", tenantId.ToString());

            identity.SetScopes(scopes);

            var resources = new List<string>();
            await foreach (var resource in _scopeManager.ListResourcesAsync(identity.GetScopes()))
                resources.Add(resource);

            identity.SetResources(resources);

            identity.SetDestinations(GetDestinations);

            return principal;
        }

        //  DESTINATIONS 
        private static IEnumerable<string> GetDestinations(Claim claim)
        {
            switch (claim.Type)
            {
                case Claims.Subject:
                case Claims.Name:
                case Claims.Email:
                case "display_name":
                case "tenant_id":
                    yield return Destinations.AccessToken;
                    yield return Destinations.IdentityToken;
                    break;

                case Claims.Role:
                    yield return Destinations.AccessToken;
                    break;

                default:
                    yield return Destinations.AccessToken;
                    break;
            }
        }

        [HttpPost("~/connect/logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return SignOut(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
    }
}