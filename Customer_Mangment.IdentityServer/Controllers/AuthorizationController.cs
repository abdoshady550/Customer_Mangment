using Customer_Mangment.IdentityServer.CQRS.Authorization;
using Customer_Mangment.IdentityServer.CQRS.Authorization.Commands;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using OpenIddict.Server.AspNetCore;
using Wolverine;

namespace Customer_Mangment.IdentityServer.Controllers;

[ApiController]
[Route("connect")]
public class AuthorizationController : ControllerBase
{
    private readonly IMessageBus _bus;

    public AuthorizationController(IMessageBus bus)
    {
        _bus = bus;
    }

    [HttpPost("token")]
    [EnableRateLimiting(policyName: "IpPolicy")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest();
        if (request is null)
            return BadRequest("Invalid OpenIddict request");

        OpenIddictResponses response = request.GrantType switch
        {
            OpenIdConnectGrantTypes.Password =>
                await _bus.InvokeAsync<OpenIddictResponses>(new PasswordGrantCommand(request, HttpContext)),

            OpenIdConnectGrantTypes.ClientCredentials =>
                await _bus.InvokeAsync<OpenIddictResponses>(new ClientCredentialsTokenCommand(request, HttpContext)),

            OpenIdConnectGrantTypes.RefreshToken =>
                await _bus.InvokeAsync<OpenIddictResponses>(new RefreshTokenGrantCommand(request, HttpContext)),

            _ => new OpenIddictResponses { IsForbidden = true }
        };

        if (response.IsForbidden)
            return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        return SignIn(response.Principal!, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        // Wolverine could also handle logout, but for simplicity we keep it inline
        await HttpContext.SignOutAsync();
        return SignOut(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
}