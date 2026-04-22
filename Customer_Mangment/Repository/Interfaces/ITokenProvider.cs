using Customer_Mangment.CQRS.Identity.Dto;
using System.Security.Claims;

namespace Customer_Mangment.Repository.Interfaces
{
    public interface ITokenProvider
    {
        Task<Model.Results.Result<TokenResponse>> GenerateJwtTokenAsync(AppUserDto user, CancellationToken ct = default);

        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}