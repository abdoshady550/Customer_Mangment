// ─────────────────────────────────────────────────────────────────────────────
// NSwag-style typed client for Customer_Mangment.IdentityServer
// Endpoints: POST /connect/token  |  POST /connect/logout
//            POST /api/users/register
//            GET  /api/users/{userId}
//            POST /api/users/change-password
// ─────────────────────────────────────────────────────────────────────────────

using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace IdentityServer.Integration.Tests.Common
{
    // ── Request / Response models ─────────────────────────────────────────────

    public class PasswordTokenRequest
    {
        public string Username  { get; set; } = "";
        public string Password  { get; set; } = "";
        public string ClientId  { get; set; } = "customer-management-swagger";
        public string Scope     { get; set; } = "customer_api offline_access roles";
    }

    public class TokenResponse
    {
        [JsonProperty("access_token")]  public string AccessToken  { get; set; } = "";
        [JsonProperty("refresh_token")] public string RefreshToken { get; set; } = "";
        [JsonProperty("expires_in")]    public int    ExpiresIn    { get; set; }
        [JsonProperty("token_type")]    public string TokenType    { get; set; } = "";
        [JsonProperty("scope")]         public string Scope        { get; set; } = "";
    }

    public class RegisterRequest
    {
        [JsonProperty("email")]       public string Email       { get; set; } = "";
        [JsonProperty("password")]    public string Password    { get; set; } = "";
        [JsonProperty("displayName")] public string DisplayName { get; set; } = "";
    }

    public class RegisterResponse
    {
        [JsonProperty("id")]    public string Id    { get; set; } = "";
        [JsonProperty("email")] public string Email { get; set; } = "";
    }

    public class UserResponse
    {
        [JsonProperty("id")]          public string Id          { get; set; } = "";
        [JsonProperty("email")]       public string Email       { get; set; } = "";
        [JsonProperty("displayName")] public string DisplayName { get; set; } = "";
    }

    public class ChangePasswordRequest
    {
        [JsonProperty("currentPassword")] public string CurrentPassword { get; set; } = "";
        [JsonProperty("newPassword")]     public string NewPassword     { get; set; } = "";
    }

    // ── Exception ─────────────────────────────────────────────────────────────

    public class ApiException : Exception
    {
        public int    StatusCode { get; }
        public string Response   { get; }

        public ApiException(string message, int statusCode, string response, Exception? inner = null)
            : base($"{message}\n\nStatus: {statusCode}\nResponse: {response}", inner)
        {
            StatusCode = statusCode;
            Response   = response;
        }
    }

    // ── Client ────────────────────────────────────────────────────────────────

    public class IdentityClient
    {
        private readonly HttpClient _http;
        private static readonly JsonSerializerSettings _json = new();

        public IdentityClient(HttpClient http) => _http = http;

        // ── /connect/token  (password grant) ─────────────────────────────────

        public Task<TokenResponse> GetPasswordTokenAsync(PasswordTokenRequest req,
            CancellationToken ct = default)
            => GetPasswordTokenAsync(req.Username, req.Password, req.ClientId, req.Scope, ct);

        public async Task<TokenResponse> GetPasswordTokenAsync(
            string username, string password,
            string clientId = "customer-management-swagger",
            string scope    = "customer_api offline_access roles",
            CancellationToken ct = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "connect/token");
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["username"]   = username,
                ["password"]   = password,
                ["client_id"]  = clientId,
                ["scope"]      = scope
            });

            var response = await _http.SendAsync(request, ct).ConfigureAwait(false);
            var body     = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

            if ((int)response.StatusCode == 200)
                return JsonConvert.DeserializeObject<TokenResponse>(body, _json)
                    ?? throw new ApiException("Null token response", 200, body);

            throw new ApiException("Token request failed", (int)response.StatusCode, body);
        }

        // ── /connect/token  (refresh_token grant) ────────────────────────────

        public async Task<TokenResponse> RefreshTokenAsync(
            string refreshToken,
            string clientId = "customer-management-swagger",
            CancellationToken ct = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "connect/token");
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"]    = "refresh_token",
                ["refresh_token"] = refreshToken,
                ["client_id"]     = clientId
            });

            var response = await _http.SendAsync(request, ct).ConfigureAwait(false);
            var body     = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

            if ((int)response.StatusCode == 200)
                return JsonConvert.DeserializeObject<TokenResponse>(body, _json)
                    ?? throw new ApiException("Null token response", 200, body);

            throw new ApiException("Refresh token request failed", (int)response.StatusCode, body);
        }

        // ── /connect/logout ───────────────────────────────────────────────────

        public async Task LogoutAsync(string accessToken, CancellationToken ct = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "connect/logout");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = new StringContent("", Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(request, ct).ConfigureAwait(false);
            var body     = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

            if ((int)response.StatusCode is not (200 or 302))
                throw new ApiException("Logout failed", (int)response.StatusCode, body);
        }

        // ── POST /api/users/register ──────────────────────────────────────────

        public async Task<RegisterResponse> RegisterAsync(
            RegisterRequest body, CancellationToken ct = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "api/users/register");
            request.Content = new StringContent(
                JsonConvert.SerializeObject(body, _json), Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(request, ct).ConfigureAwait(false);
            var raw      = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

            if ((int)response.StatusCode == 200)
                return JsonConvert.DeserializeObject<RegisterResponse>(raw, _json)
                    ?? throw new ApiException("Null register response", 200, raw);

            throw new ApiException("Register failed", (int)response.StatusCode, raw);
        }

        // ── GET /api/users/{userId} ───────────────────────────────────────────

        public async Task<UserResponse> GetUserAsync(
            string userId, string accessToken, CancellationToken ct = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"api/users/{userId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _http.SendAsync(request, ct).ConfigureAwait(false);
            var raw      = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

            if ((int)response.StatusCode == 200)
                return JsonConvert.DeserializeObject<UserResponse>(raw, _json)
                    ?? throw new ApiException("Null user response", 200, raw);

            throw new ApiException("GetUser failed", (int)response.StatusCode, raw);
        }

        // ── POST /api/users/change-password ───────────────────────────────────

        public async Task ChangePasswordAsync(
            ChangePasswordRequest body, string accessToken, CancellationToken ct = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "api/users/change-password");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = new StringContent(
                JsonConvert.SerializeObject(body, _json), Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(request, ct).ConfigureAwait(false);
            var raw      = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

            if ((int)response.StatusCode != 200)
                throw new ApiException("ChangePassword failed", (int)response.StatusCode, raw);
        }
    }
}
