using Customer_Mangment;
using Customer_Mangment_Integrate.Test.Common;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Customer_Mangment_Integrate.Test
{
    /// <summary>
    /// Authentication tests targeting the OpenIddict password-grant flow
    /// (POST /connect/token). All token calls go directly to the test server
    /// via raw HttpClient — no dependency on the Client partial-class extensions.
    /// </summary>
    public class AuthTests : TestBase
    {
        public AuthTests(WebApplicationFactory<IAssmblyMarker> factory) : base(factory) { }

        // ── Raw helpers ───────────────────────────────────────────────────

        private record TokenResult(
            string? AccessToken, string? RefreshToken, int StatusCode, bool IsSuccess);

        private async Task<TokenResult> RequestPasswordTokenRawAsync(
            string email, string password, string? tenantId = null)
        {
            var http = _factory.CreateClient();
            if (!string.IsNullOrEmpty(tenantId))
                http.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId);

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["username"] = email,
                ["password"] = password,
                ["client_id"] = "customer-management-swagger",
                ["scope"] = "customer_api offline_access roles"
            });

            var response = await http.PostAsync("connect/token", content);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return new TokenResult(null, null, (int)response.StatusCode, false);

            var doc = JsonDocument.Parse(body);
            var access = doc.RootElement.GetProperty("access_token").GetString();
            var refresh = doc.RootElement.TryGetProperty("refresh_token", out var rt)
                ? rt.GetString() : null;

            return new TokenResult(access, refresh, (int)response.StatusCode, true);
        }

        private async Task<TokenResult> RequestRefreshTokenRawAsync(string refreshToken)
        {
            var http = _factory.CreateClient();

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken,
                ["client_id"] = "customer-management-swagger"
            });

            var response = await http.PostAsync("connect/token", content);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return new TokenResult(null, null, (int)response.StatusCode, false);

            var doc = JsonDocument.Parse(body);
            var access = doc.RootElement.GetProperty("access_token").GetString();
            var refresh = doc.RootElement.TryGetProperty("refresh_token", out var rt)
                ? rt.GetString() : null;

            return new TokenResult(access, refresh, (int)response.StatusCode, true);
        }

        // ── Password grant 

        [Fact]
        public async Task PasswordGrant_AdminValidCredentials_ReturnsAccessAndRefreshToken()
        {
            var result = await RequestPasswordTokenRawAsync(AdminEmail, AdminPassword);

            Assert.True(result.IsSuccess, $"Expected success but got {result.StatusCode}");
            Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
            Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
        }

        [Fact]
        public async Task PasswordGrant_UserValidCredentials_ReturnsAccessAndRefreshToken()
        {
            var result = await RequestPasswordTokenRawAsync(UserEmail, UserPassword);

            Assert.True(result.IsSuccess, $"Expected success but got {result.StatusCode}");
            Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
            Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
        }

        [Fact]
        public async Task PasswordGrant_WrongPassword_ReturnsFailure()
        {
            var result = await RequestPasswordTokenRawAsync(AdminEmail, "WrongPassword!");

            Assert.False(result.IsSuccess);
            Assert.True(result.StatusCode == 400 || result.StatusCode == 401,
                $"Expected 400 or 401 but got {result.StatusCode}");
        }

        [Fact]
        public async Task PasswordGrant_UnknownEmail_ReturnsFailure()
        {
            var result = await RequestPasswordTokenRawAsync("nobody@nowhere.com", "Test@123");

            Assert.False(result.IsSuccess);
            Assert.True(result.StatusCode == 400 || result.StatusCode == 401,
                $"Expected 400 or 401 but got {result.StatusCode}");
        }

        [Fact]
        public async Task PasswordGrant_EmptyPassword_ReturnsFailure()
        {
            var result = await RequestPasswordTokenRawAsync(AdminEmail, "");

            Assert.False(result.IsSuccess);
            Assert.True(result.StatusCode == 400 || result.StatusCode == 401);
        }

        [Fact]
        public async Task PasswordGrant_AdminAndUser_ReturnDifferentAccessTokens()
        {
            var admin = await RequestPasswordTokenRawAsync(AdminEmail, AdminPassword);
            var user = await RequestPasswordTokenRawAsync(UserEmail, UserPassword);

            Assert.True(admin.IsSuccess);
            Assert.True(user.IsSuccess);
            Assert.NotEqual(admin.AccessToken, user.AccessToken);
        }

        [Fact]
        public async Task PasswordGrant_AdminToken_AllowsAccessToProtectedEndpoint()
        {
            var result = await RequestPasswordTokenRawAsync(
                AdminEmail, AdminPassword, DefaultTenantId);

            Assert.True(result.IsSuccess);

            var apiClient = CreateApiClient(result.AccessToken!);
            var customers = await apiClient.Get2Async(null);
            Assert.NotNull(customers);
        }

        [Fact]
        public async Task PasswordGrant_WithTenantHeader_TokenCarriesTenantClaim()
        {
            var token = await GetAdminTokenAsync();
            var client = CreateApiClient(token);

            var customers = await client.Get2Async(null);
            Assert.NotNull(customers);
        }

        // ── Refresh grant 

        [Fact]
        public async Task RefreshGrant_WithValidRefreshToken_ReturnsNewTokenPair()
        {
            var initial = await RequestPasswordTokenRawAsync(AdminEmail, AdminPassword);
            Assert.True(initial.IsSuccess);

            var refreshed = await RequestRefreshTokenRawAsync(initial.RefreshToken!);

            Assert.True(refreshed.IsSuccess, $"Refresh failed with {refreshed.StatusCode}");
            Assert.False(string.IsNullOrWhiteSpace(refreshed.AccessToken));
            Assert.False(string.IsNullOrWhiteSpace(refreshed.RefreshToken));
        }

        [Fact]
        public async Task RefreshGrant_NewTokensDifferFromOriginal()
        {
            var initial = await RequestPasswordTokenRawAsync(AdminEmail, AdminPassword);
            var refreshed = await RequestRefreshTokenRawAsync(initial.RefreshToken!);

            Assert.NotEqual(initial.AccessToken, refreshed.AccessToken);
            Assert.NotEqual(initial.RefreshToken, refreshed.RefreshToken);
        }

        [Fact]
        public async Task RefreshGrant_NewTokenAllowsAccessToProtectedEndpoint()
        {
            var initial = await RequestPasswordTokenRawAsync(
                AdminEmail, AdminPassword, DefaultTenantId);
            var refreshed = await RequestRefreshTokenRawAsync(initial.RefreshToken!);

            Assert.True(refreshed.IsSuccess);

            var apiClient = CreateApiClient(refreshed.AccessToken!);
            var customers = await apiClient.Get2Async(null);
            Assert.NotNull(customers);
        }

        [Fact]
        public async Task RefreshGrant_InvalidRefreshToken_ReturnsFailure()
        {
            var result = await RequestRefreshTokenRawAsync("totally-invalid-refresh-token");

            Assert.False(result.IsSuccess);
            Assert.True(result.StatusCode == 400 || result.StatusCode == 401,
                $"Expected 400 or 401 but got {result.StatusCode}");
        }

        [Fact]
        public async Task RefreshGrant_EmptyRefreshToken_ReturnsFailure()
        {
            var result = await RequestRefreshTokenRawAsync("");

            Assert.False(result.IsSuccess);
            Assert.True(result.StatusCode == 400 || result.StatusCode == 401);
        }

        [Fact]
        public async Task RefreshGrant_UserToken_ReturnsNewPair()
        {
            var initial = await RequestPasswordTokenRawAsync(UserEmail, UserPassword);
            var refreshed = await RequestRefreshTokenRawAsync(initial.RefreshToken!);

            Assert.True(refreshed.IsSuccess);
            Assert.False(string.IsNullOrWhiteSpace(refreshed.AccessToken));
        }

        // ── Token / tenant interaction 

        [Fact]
        public async Task Token_IssuedForDemoTenant_Rejected_WhenUsedWithDifferentTenant()
        {
            var token = await GetAdminTokenAsync();

            var http = _factory.CreateClient();
            http.DefaultRequestHeaders.Add("X-Tenant-Id", "alahly");
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await http.GetAsync("api/Customer/get");
            Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Token_UsedAgainstCorrectTenant_Succeeds()
        {
            var token = await GetAdminTokenAsync();
            var client = CreateApiClient(token);

            var customers = await client.Get2Async(null);
            Assert.NotNull(customers);
        }

        [Fact]
        public async Task NoToken_Returns401OnProtectedEndpoint()
        {
            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => CreateApiClient().Get2Async(null));

            Assert.Equal(401, ex.StatusCode);
        }
    }
}
