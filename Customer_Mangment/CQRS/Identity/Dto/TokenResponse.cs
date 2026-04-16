using System.Text.Json.Serialization;

namespace Customer_Mangment.CQRS.Identity.Dto
{
    public class TokenResponse
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime ExpiresOnUtc { get; set; }
    }
    public record TokenServerResponse
    {
        [JsonPropertyName("access_token")] public string AccessToken { get; init; } = string.Empty;
        [JsonPropertyName("token_type")] public string TokenType { get; init; } = string.Empty;
        [JsonPropertyName("expires_in")] public int ExpiresIn { get; init; }
        [JsonPropertyName("refresh_token")] public string? RefreshToken { get; init; }
    }
}
