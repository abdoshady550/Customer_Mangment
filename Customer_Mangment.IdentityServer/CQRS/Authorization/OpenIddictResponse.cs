using System.Security.Claims;

namespace Customer_Mangment.IdentityServer.CQRS.Authorization
{
    public class OpenIddictResponses
    {
        public ClaimsPrincipal? Principal { get; init; }
        public string? AuthenticationScheme { get; init; }
        public bool IsForbidden { get; init; }
    }
}
