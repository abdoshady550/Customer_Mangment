using System.Text.Json.Serialization;

namespace RabbitMQ_Client_Console.DTOs
{
    // ── DTOs ────────────────────────────────────────────────────────────────────

    public sealed record TokenServerResponse(
        [property: JsonPropertyName("access_token")] string? AccessToken,
        [property: JsonPropertyName("token_type")] string? TokenType,
        [property: JsonPropertyName("expires_in")] int ExpiresIn,
        [property: JsonPropertyName("refresh_token")] string? RefreshToken);
}
