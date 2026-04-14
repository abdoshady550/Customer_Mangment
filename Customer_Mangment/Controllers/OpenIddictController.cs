using Asp.Versioning;
using Customer_Mangment.Repository.Interfaces;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Collections.Immutable;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Customer_Mangment.Controllers;

[ApiController]
[ApiVersion("1.0")]
public sealed class OpenIddictController(
    IIdentityService identityService,
    IConfiguration configuration) : ControllerBase
{
    private readonly IIdentityService _identityService = identityService;
    private readonly IConfiguration _configuration = configuration;

    [HttpPost("~/connect/token")]
    [Produces("application/json")]
    public async Task<IActionResult> Exchange(CancellationToken ct)
    {
        var request = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        if (request.IsPasswordGrantType())
        {
            var result = await _identityService.AuthenticateAsync(request.Username ?? string.Empty, request.Password ?? string.Empty);
            if (result.IsError)
            {
                return Forbid(
                    CreateErrorProperties(Errors.InvalidGrant, "The username/password couple is invalid."),
                    OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            var principal = await CreatePrincipalAsync(result.Value.UserId, request.GetScopes(), ct);
            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        if (request.IsRefreshTokenGrantType())
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            if (authenticateResult.Succeeded is not true || authenticateResult.Principal is null)
            {
                return Forbid(
                    CreateErrorProperties(Errors.InvalidGrant, "The refresh token is no longer valid."),
                    OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            var subject = authenticateResult.Principal.FindFirstValue(Claims.Subject);
            if (string.IsNullOrWhiteSpace(subject))
            {
                return Forbid(
                    CreateErrorProperties(Errors.InvalidGrant, "The refresh token subject is invalid."),
                    OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            var userResult = await _identityService.GetUserByIdAsync(subject);
            if (userResult.IsError)
            {
                return Forbid(
                    CreateErrorProperties(Errors.InvalidGrant, "The user associated with the refresh token no longer exists."),
                    OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            var principal = await CreatePrincipalAsync(userResult.Value.UserId, request.GetScopes(), ct);
            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        return BadRequest(new
        {
            error = Errors.UnsupportedGrantType,
            error_description = "Only the password and refresh_token grants are enabled in this incremental setup."
        });
    }

    private async Task<ClaimsPrincipal> CreatePrincipalAsync(string userId, ImmutableArray<string> requestedScopes, CancellationToken ct)
    {
        _ = ct;

        var userResult = await _identityService.GetUserByIdAsync(userId);
        if (userResult.IsError)
        {
            throw new InvalidOperationException("The user could not be loaded.");
        }

        var user = userResult.Value;
        var identity = new ClaimsIdentity(
            TokenValidationParameters.DefaultAuthenticationType,
            Claims.Name,
            Claims.Role);

        identity.AddClaim(new Claim(Claims.Subject, user.UserId));
        identity.AddClaim(new Claim(Claims.Name, user.Email));
        identity.AddClaim(new Claim(Claims.Email, user.Email));
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.UserId));
        identity.AddClaim(new Claim(ClaimTypes.Name, user.Email));
        identity.AddClaim(new Claim(ClaimTypes.Email, user.Email));

        foreach (var role in user.Roles.Distinct())
        {
            identity.AddClaim(new Claim(Claims.Role, role));
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }

        var tenantId = HttpContext.Request.Headers["X-Tenant-Id"].FirstOrDefault()?.Trim();
        if (!string.IsNullOrWhiteSpace(tenantId))
        {
            identity.AddClaim(new Claim("tenant_id", tenantId));
        }

        var principal = new ClaimsPrincipal(identity);
        var scopes = requestedScopes.IsDefaultOrEmpty
            ? new[] { "api", Scopes.OfflineAccess }
            : requestedScopes.Distinct().ToArray();

        principal.SetScopes(scopes);
        principal.SetResources(_configuration["OpenIddict:ApiResourceName"] ?? "customer_management_api");
        principal.SetDestinations(GetDestinations);

        return principal;
    }

    private static AuthenticationProperties CreateErrorProperties(string error, string description)
    {
        return new AuthenticationProperties(new Dictionary<string, string?>
        {
            [OpenIddictServerAspNetCoreConstants.Properties.Error] = error,
            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = description
        });
    }

    private static IEnumerable<string> GetDestinations(Claim claim)
    {
        yield return Destinations.AccessToken;

        if (claim.Type is Claims.Email or Claims.Subject or Claims.Role)
        {
            yield return Destinations.IdentityToken;
        }
    }
}
