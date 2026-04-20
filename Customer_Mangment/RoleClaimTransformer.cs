using Customer_Mangment.Model.Entities;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace Customer_Mangment
{
    public sealed class RoleClaimTransformer : IClaimsTransformation
    {

        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal.HasClaim("roles_expanded", "1"))
                return Task.FromResult(principal);

            var identity = (ClaimsIdentity)principal.Identity!;


            var roleClaimTypes = new[]
            {
                ClaimTypes.Role,
                "role"
            };

            var roleClaims = roleClaimTypes
                .SelectMany(t => identity.FindAll(t))
                .ToList();

            bool anyNumeric = false;

            foreach (var claim in roleClaims)
            {
                if (int.TryParse(claim.Value, out var index)
                    && Enum.IsDefined(typeof(Role), index))
                {
                    var name = ((Role)index).ToString();
                    identity.RemoveClaim(claim);
                    identity.AddClaim(new Claim(claim.Type, name));
                    anyNumeric = true;
                }
            }

            if (anyNumeric)
                identity.AddClaim(new Claim("roles_expanded", "1"));

            return Task.FromResult(principal);
        }
    }
}
