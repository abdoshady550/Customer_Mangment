using System.Text.Json.Serialization;

namespace Customer_Mangment.Repository.Interfaces
{
    public interface IIdentityServerTokenService
    {
        Task<TokenServerResponse?> RequestPasswordTokenAsync(string email, string password, CancellationToken ct);
        Task<TokenServerResponse?> RefreshTokenAsync(string refreshToken, CancellationToken ct);
    }

    public record TokenServerResponse
    {
        [JsonPropertyName("access_token")] public string AccessToken { get; init; } = string.Empty;
        [JsonPropertyName("token_type")] public string TokenType { get; init; } = string.Empty;
        [JsonPropertyName("expires_in")] public int ExpiresIn { get; init; }
        [JsonPropertyName("refresh_token")] public string? RefreshToken { get; init; }
    }
}
