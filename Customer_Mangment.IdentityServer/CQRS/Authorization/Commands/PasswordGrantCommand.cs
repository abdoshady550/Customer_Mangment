using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;

namespace Customer_Mangment.IdentityServer.CQRS.Authorization.Commands
{
    public sealed record PasswordGrantCommand(
           OpenIddictRequest Request,
    HttpContext HttpContext) : IIdentityRequest<IActionResult>;

    public sealed record LogoutCommand : IIdentityRequest<IActionResult>;
}
