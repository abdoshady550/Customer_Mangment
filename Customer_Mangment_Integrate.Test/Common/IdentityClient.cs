using Newtonsoft.Json;

namespace Customer_Mangment_Integrate.Test.Common
{

    public class IdentityClient
    {
        private readonly HttpClient _http;
        private static readonly JsonSerializerSettings _json = new();

        public IdentityClient(HttpClient http) => _http = http;

        // ── password grant ────────────────────────────────────────────────────

        public async Task<IdentityTokenResponse> GetPasswordTokenAsync(
            string username,
            string password,
            string clientId = "customer-management-swagger",
            string scope = "customer_api offline_access roles",
            string? tenantId = null,
            CancellationToken ct = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "connect/token");

            if (!string.IsNullOrWhiteSpace(tenantId))
                request.Headers.Add("X-Tenant-Id", tenantId);

            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["username"] = username,
                ["password"] = password,
                ["client_id"] = clientId,
                ["scope"] = scope
            });

            var response = await _http.SendAsync(request, ct).ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

            if ((int)response.StatusCode == 200)
                return JsonConvert.DeserializeObject<IdentityTokenResponse>(body, _json)
                    ?? throw new IdentityClientException("Null token response", 200, body);

            throw new IdentityClientException("Token request failed", (int)response.StatusCode, body);
        }

        // ── refresh_token grant ───────────────────────────────────────────────

        public async Task<IdentityTokenResponse> RefreshTokenAsync(
            string refreshToken,
            string clientId = "customer-management-swagger",
            string? tenantId = null,
            CancellationToken ct = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "connect/token");

            if (!string.IsNullOrWhiteSpace(tenantId))
                request.Headers.Add("X-Tenant-Id", tenantId);

            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken,
                ["client_id"] = clientId
            });

            var response = await _http.SendAsync(request, ct).ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

            if ((int)response.StatusCode == 200)
                return JsonConvert.DeserializeObject<IdentityTokenResponse>(body, _json)
                    ?? throw new IdentityClientException("Null token response", 200, body);

            throw new IdentityClientException("Refresh token request failed", (int)response.StatusCode, body);
        }
    }

    // ── Response model ────────────────────────────────────────────────────────

    public class IdentityTokenResponse
    {
        [JsonProperty("access_token")] public string AccessToken { get; set; } = "";
        [JsonProperty("refresh_token")] public string RefreshToken { get; set; } = "";
        [JsonProperty("expires_in")] public int ExpiresIn { get; set; }
        [JsonProperty("token_type")] public string TokenType { get; set; } = "";
        [JsonProperty("scope")] public string Scope { get; set; } = "";
    }

    // ── Exception ─────────────────────────────────────────────────────────────

    public class IdentityClientException : Exception
    {
        public int StatusCode { get; }
        public string Response { get; }

        public IdentityClientException(string message, int statusCode, string response, Exception? inner = null)
            : base($"{message} | Status: {statusCode} | Response: {response}", inner)
        {
            StatusCode = statusCode;
            Response = response;
        }
    }
}