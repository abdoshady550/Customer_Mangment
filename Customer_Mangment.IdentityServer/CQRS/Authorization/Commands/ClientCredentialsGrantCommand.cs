using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;

namespace Customer_Mangment.IdentityServer.CQRS.Authorization.Commands
{
    public record ClientCredentialsTokenCommand(
     OpenIddictRequest Request,
     HttpContext HttpContext
 ) : IIdentityRequest<IActionResult>;
}
