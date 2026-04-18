using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;

namespace Customer_Mangment.IdentityServer.CQRS.Authorization.Commands
{
    public sealed record RefreshTokenGrantCommand(
        OpenIddictRequest Request, HttpContext HttpContext
) : IIdentityRequest<IActionResult>;
}
