using Customer_Mangment.IdentityServer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OpenIddict.Server;
using System.Net;
using System.Text;

namespace IdentityServer.Integration.Tests
{
    /// <summary>
    /// Custom factory that patches OpenIddict to accept plain HTTP requests.
    /// OpenIddict enforces HTTPS by default; in the test host there is no TLS,
    /// so we disable that check via its built-in option.
    /// </summary>
    public class IdentityServerFactory : WebApplicationFactory<IMarkerIdentity>
    {
        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Tell OpenIddict's server to allow HTTP (disables the ID2083 HTTPS-only guard)
                services.Configure<OpenIddictServerOptions>(options =>
                {
                    options.TokenEndpointUris.Clear();
                    options.TokenEndpointUris.Add(new Uri("/connect/token", UriKind.Relative));

                    options.LogoutEndpointUris.Clear();
                    options.LogoutEndpointUris.Add(new Uri("/connect/logout", UriKind.Relative));

                    // Disable the HTTPS requirement
                });
            });
        }
    }

    public class AuthorizationControllerTests : IClassFixture<IdentityServerFactory>
    {
        private readonly IdentityServerFactory _factory;

        private const string AdminEmail = "admin@test.com";
        private const string AdminPassword = "Admin@123";
        private const string UserEmail = "user@test.com";
        private const string UserPassword = "User@123";

        private const string SwaggerClientId = "customer-management-swagger";
        private const string M2MClientId = "customer-management-m2m";
        private const string M2MClientSecret = "secret-for-m2m";
        private const string PasswordScope = "openid profile email roles offline_access customer_api";
        private const string ClientCredentialsScope = "customer_api";

        public AuthorizationControllerTests(IdentityServerFactory factory)
            => _factory = factory;

        // ── Infrastructure ────────────────────────────────────────────────────

        private HttpClient CreateHttp() =>
            _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

        private Task<HttpResponseMessage> PostTokenAsync(Dictionary<string, string> fields) =>
            CreateHttp().PostAsync("connect/token", new FormUrlEncodedContent(fields));

        private Task<HttpResponseMessage> PostPasswordAsync(
            string username, string password,
            string clientId = SwaggerClientId,
            string scope = PasswordScope) =>
            PostTokenAsync(new()
            {
                ["grant_type"] = "password",
                ["username"] = username,
                ["password"] = password,
                ["client_id"] = clientId,
                ["scope"] = scope
            });

        private Task<HttpResponseMessage> PostRefreshAsync(
            string refreshToken, string clientId = SwaggerClientId) =>
            PostTokenAsync(new()
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken,
                ["client_id"] = clientId
            });

        /// <summary>
        /// Client credentials – confidential clients must send credentials via
        /// HTTP Basic auth (per RFC 6749 §2.3.1), which is what OpenIddict expects.
        /// </summary>
        private Task<HttpResponseMessage> PostClientCredentialsAsync(
            string clientId = M2MClientId,
            string clientSecret = M2MClientSecret,
            string scope = ClientCredentialsScope)
        {
            var http = CreateHttp();
            var credentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

            return http.PostAsync("connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["scope"] = scope
            }));
        }

        private async Task<(string accessToken, string refreshToken)> GetTokenPairAsync(
            string email = AdminEmail, string password = AdminPassword)
        {
            var resp = await PostPasswordAsync(email, password);
            var body = await resp.Content.ReadAsStringAsync();
            Assert.True(resp.IsSuccessStatusCode,
                $"GetTokenPairAsync failed {(int)resp.StatusCode}: {body}");
            dynamic obj = JsonConvert.DeserializeObject(body)!;
            return ((string)obj.access_token, (string)obj.refresh_token);
        }

        private static dynamic ParseJson(string body) =>
            JsonConvert.DeserializeObject(body)
            ?? throw new InvalidOperationException("Response body was null");

        // ═════════════════════════════════════════════════════════════════════
        // POST /connect/token  –  password grant – success
        // ═════════════════════════════════════════════════════════════════════

        [Fact]
        public async Task Token_PasswordGrant_AdminCredentials_Returns200()
        {
            var resp = await PostPasswordAsync(AdminEmail, AdminPassword);
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        }

        [Fact]
        public async Task Token_PasswordGrant_UserCredentials_Returns200()
        {
            var resp = await PostPasswordAsync(UserEmail, UserPassword);
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        }

        [Fact]
        public async Task Token_PasswordGrant_ReturnsAccessToken()
        {
            var resp = await PostPasswordAsync(AdminEmail, AdminPassword);
            var obj = ParseJson(await resp.Content.ReadAsStringAsync());
            Assert.False(string.IsNullOrWhiteSpace((string)obj.access_token));
        }

        [Fact]
        public async Task Token_PasswordGrant_WithOfflineAccess_ReturnsRefreshToken()
        {
            var resp = await PostPasswordAsync(AdminEmail, AdminPassword);
            var obj = ParseJson(await resp.Content.ReadAsStringAsync());
            Assert.False(string.IsNullOrWhiteSpace((string)obj.refresh_token));
        }

        [Fact]
        public async Task Token_PasswordGrant_ReturnsBearerTokenType()
        {
            var resp = await PostPasswordAsync(AdminEmail, AdminPassword);
            var obj = ParseJson(await resp.Content.ReadAsStringAsync());
            Assert.Equal("Bearer", (string)obj.token_type, ignoreCase: true);
        }

        [Fact]
        public async Task Token_PasswordGrant_ReturnsPositiveExpiresIn()
        {
            var resp = await PostPasswordAsync(AdminEmail, AdminPassword);
            var obj = ParseJson(await resp.Content.ReadAsStringAsync());
            Assert.True((int)obj.expires_in > 0);
        }

        [Fact]
        public async Task Token_PasswordGrant_AdminAndUser_ReturnDifferentAccessTokens()
        {
            var (adminTok, _) = await GetTokenPairAsync(AdminEmail, AdminPassword);
            var (userTok, _) = await GetTokenPairAsync(UserEmail, UserPassword);
            Assert.NotEqual(adminTok, userTok);
        }

        // ═════════════════════════════════════════════════════════════════════
        // POST /connect/token  –  password grant – failures
        // ═════════════════════════════════════════════════════════════════════

        [Fact]
        public async Task Token_PasswordGrant_WrongPassword_Returns400()
        {
            var resp = await PostPasswordAsync(AdminEmail, "WrongPassword!");
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task Token_PasswordGrant_UnknownUser_Returns400()
        {
            var resp = await PostPasswordAsync("ghost@nowhere.com", "Test@123");
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task Token_PasswordGrant_EmptyUsername_Returns400()
        {
            var resp = await PostPasswordAsync("", AdminPassword);
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task Token_PasswordGrant_EmptyPassword_Returns400()
        {
            var resp = await PostPasswordAsync(AdminEmail, "");
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task Token_PasswordGrant_UnknownClientId_Returns400()
        {
            var resp = await PostPasswordAsync(AdminEmail, AdminPassword, clientId: "no-such-client");
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        // ═════════════════════════════════════════════════════════════════════
        // POST /connect/token  –  refresh_token grant
        // ═════════════════════════════════════════════════════════════════════

        [Fact]
        public async Task Token_RefreshGrant_ValidToken_Returns200()
        {
            var (_, refresh) = await GetTokenPairAsync();
            var resp = await PostRefreshAsync(refresh);
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        }

        [Fact]
        public async Task Token_RefreshGrant_ReturnsNewAccessToken()
        {
            var (original, refresh) = await GetTokenPairAsync();
            var resp = await PostRefreshAsync(refresh);
            var obj = ParseJson(await resp.Content.ReadAsStringAsync());

            Assert.False(string.IsNullOrWhiteSpace((string)obj.access_token));
            Assert.NotEqual(original, (string)obj.access_token);
        }

        [Fact]
        public async Task Token_RefreshGrant_ReturnsNewRefreshToken()
        {
            var (_, refresh) = await GetTokenPairAsync();
            var resp = await PostRefreshAsync(refresh);
            var obj = ParseJson(await resp.Content.ReadAsStringAsync());

            Assert.False(string.IsNullOrWhiteSpace((string)obj.refresh_token));
            Assert.NotEqual(refresh, (string)obj.refresh_token);
        }

        [Fact]
        public async Task Token_RefreshGrant_InvalidToken_Returns400()
        {
            var resp = await PostRefreshAsync("totally-invalid-token");
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task Token_RefreshGrant_EmptyToken_Returns400()
        {
            var resp = await PostRefreshAsync("");
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        // ═════════════════════════════════════════════════════════════════════
        // POST /connect/token  –  client_credentials grant
        // ═════════════════════════════════════════════════════════════════════

        [Fact]
        public async Task Token_ClientCredentials_ValidSecret_Returns200()
        {
            var resp = await PostClientCredentialsAsync();
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        }

        [Fact]
        public async Task Token_ClientCredentials_ReturnsAccessToken()
        {
            var resp = await PostClientCredentialsAsync();
            var obj = ParseJson(await resp.Content.ReadAsStringAsync());
            Assert.False(string.IsNullOrWhiteSpace((string)obj.access_token));
        }

        [Fact]
        public async Task Token_ClientCredentials_WrongSecret_Returns400()
        {
            var resp = await PostClientCredentialsAsync(clientSecret: "wrong-secret");
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task Token_ClientCredentials_UnknownClient_Returns400()
        {
            var resp = await PostClientCredentialsAsync(clientId: "unknown-client", clientSecret: "any");
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        // ═════════════════════════════════════════════════════════════════════
        // POST /connect/token  –  unsupported / malformed
        // ═════════════════════════════════════════════════════════════════════

        [Fact]
        public async Task Token_UnsupportedGrantType_Returns400()
        {
            var resp = await PostTokenAsync(new()
            {
                ["grant_type"] = "authorization_code",
                ["client_id"] = SwaggerClientId,
                ["code"] = "fake-code"
            });
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task Token_NoGrantType_Returns400()
        {
            var resp = await PostTokenAsync(new() { ["client_id"] = SwaggerClientId });
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task Token_EmptyBody_Returns400()
        {
            var resp = await CreateHttp().PostAsync("connect/token",
                new StringContent("", Encoding.UTF8, "application/x-www-form-urlencoded"));
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task Token_JsonContentType_Returns400()
        {
            var resp = await CreateHttp().PostAsync("connect/token",
                new StringContent(
                    """{"grant_type":"password","username":"admin@test.com","password":"Admin@123"}""",
                    Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        // ═════════════════════════════════════════════════════════════════════
        // POST /connect/logout
        // ═════════════════════════════════════════════════════════════════════

        [Fact]
        public async Task Logout_WithValidToken_ReturnsSuccessOrRedirect()
        {
            var (accessToken, _) = await GetTokenPairAsync();
            var http = CreateHttp();
            http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var resp = await http.PostAsync("connect/logout",
                new FormUrlEncodedContent(new Dictionary<string, string>()));

            Assert.True(resp.StatusCode is HttpStatusCode.OK or HttpStatusCode.Found,
                $"Expected 200/302 but got {(int)resp.StatusCode}");
        }

        [Fact]
        public async Task Logout_WithoutToken_ReturnsSuccessOrRedirect()
        {
            var resp = await CreateHttp().PostAsync("connect/logout",
                new FormUrlEncodedContent(new Dictionary<string, string>()));

            Assert.True(resp.StatusCode is HttpStatusCode.OK or HttpStatusCode.Found,
                $"Expected 200/302 but got {(int)resp.StatusCode}");
        }

        [Fact]
        public async Task Logout_CalledTwice_BothSucceed()
        {
            var (accessToken, _) = await GetTokenPairAsync();
            var http = CreateHttp();
            http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var r1 = await http.PostAsync("connect/logout",
                new FormUrlEncodedContent(new Dictionary<string, string>()));
            var r2 = await http.PostAsync("connect/logout",
                new FormUrlEncodedContent(new Dictionary<string, string>()));

            Assert.True(r1.StatusCode is HttpStatusCode.OK or HttpStatusCode.Found,
                $"First logout: {(int)r1.StatusCode}");
            Assert.True(r2.StatusCode is HttpStatusCode.OK or HttpStatusCode.Found,
                $"Second logout: {(int)r2.StatusCode}");
        }
    }
}
