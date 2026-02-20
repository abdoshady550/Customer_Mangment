using Customer_Mangment.CQRS.Identity.Dto;
using Customer_Mangment.Model.Results;
using System.Security.Claims;

namespace Customer_Mangment.Repository.Interfaces
{
    public interface ITokenProvider
    {
        Task<Result<TokenResponse>> GenerateJwtTokenAsync(AppUserDto user, CancellationToken ct = default);

        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}