using RabbitMQ_Client_Console.DTOs;
using RabbitMQ_Client_Console.Interfaces;
using System.Text.Json;

namespace RabbitMQ_Client_Console.Services
{
    public sealed class IdentityServerTokenClient : ITokenClient
    {
        private readonly HttpClient _http;
        private readonly string _identityBaseUrl;
        private readonly string _clientId;
        private readonly string _scope;

        private static readonly JsonSerializerOptions _json =
            new() { PropertyNameCaseInsensitive = true };

        public IdentityServerTokenClient(
            HttpClient http,
            string identityBaseUrl,
            string clientId = "customer-management-swagger",
            string scope = "openid profile email roles offline_access customer_api")
        {
            _http = http;
            _identityBaseUrl = identityBaseUrl.TrimEnd('/');
            _clientId = clientId;
            _scope = scope;
        }

        public async Task<string> GetAccessTokenAsync(
            string email, string password, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty.", nameof(email));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty.", nameof(password));

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["username"] = email,
                ["password"] = password,
                ["scope"] = _scope,
                ["client_id"] = _clientId
            });

            var response = await _http.PostAsync($"{_identityBaseUrl}/connect/token", content, ct);
            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(
                    $"Token request failed [{(int)response.StatusCode} {response.StatusCode}]: {body}");

            var tokenResponse = JsonSerializer.Deserialize<TokenServerResponse>(body, _json)
                                ?? throw new InvalidOperationException("Empty token response body.");

            if (string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
                throw new InvalidOperationException(
                    $"Token endpoint returned success but access_token is missing. Body: {body}");

            return tokenResponse.AccessToken;
        }
    }
}
