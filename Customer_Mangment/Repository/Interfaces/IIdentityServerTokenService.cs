using Customer_Mangment.CQRS.Identity.Dto;

namespace Customer_Mangment.Repository.Interfaces
{
    public interface IIdentityServerTokenService
    {
        Task<TokenServerResponse?> RequestPasswordTokenAsync(string email, string password, string? tenantId, CancellationToken ct);
        Task<TokenServerResponse?> RefreshTokenAsync(string refreshToken, string? tenantId, CancellationToken ct);
    }


}
