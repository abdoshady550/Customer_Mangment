using System.Security.Claims;

namespace Customer_Mangment.CQRS.Identity.Dto
{
    public sealed record AppUserDto(string UserId, string Email, IList<string> Roles, IList<Claim> Claims);
}
