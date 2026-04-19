using Customer_Mangment;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Headers;

namespace Customer_Mangment_Integrate.Test.Common
{
    public class TestBase : IClassFixture<WebApplicationFactory<IAssmblyMarker>>
    {
        protected readonly WebApplicationFactory<IAssmblyMarker> _factory;

        protected const string AdminEmail = "admin@test.com";
        protected const string AdminPassword = "Admin@123";
        protected const string UserEmail = "user@test.com";
        protected const string UserPassword = "User@123";

        protected const string DefaultTenantId = "demo";

        private const string IdentityServerBaseUrl = "https://localhost:7278/";

        public TestBase(WebApplicationFactory<IAssmblyMarker> factory) => _factory = factory;



        protected Client CreateApiClient()
        {
            var http = CreateHttpClientWithTenant();
            return new Client(http) { BaseUrl = http.BaseAddress?.ToString() ?? "" };
        }

        protected Client CreateApiClient(string accessToken)
        {
            var http = CreateHttpClientWithTenant(accessToken);
            return new Client(http) { BaseUrl = http.BaseAddress?.ToString() ?? "" };
        }

        private HttpClient CreateHttpClientWithTenant(string? accessToken = null)
        {
            var http = _factory.CreateClient();
            http.DefaultRequestHeaders.Add("X-Tenant-Id", DefaultTenantId);
            if (!string.IsNullOrEmpty(accessToken))
                http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);
            return http;
        }
        protected async Task<string> GetAdminTokenAsync()
            => await GetTokenAsync(AdminEmail, AdminPassword, DefaultTenantId);

        protected async Task<string> GetUserTokenAsync()
            => await GetTokenAsync(UserEmail, UserPassword, DefaultTenantId);

        private async Task<string> GetTokenAsync(
            string email, string password, string tenantId)
        {
            var http = _factory.CreateClient();

            http.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId);

            var parameters = new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["username"] = email,
                ["password"] = password,
                ["client_id"] = "customer-management-swagger",
                ["scope"] = "customer_api offline_access roles"
            };

            var response = await http.PostAsync(
                "connect/token",
                new FormUrlEncodedContent(parameters));

            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException(
                    $"Token request failed ({(int)response.StatusCode}): {body}");

            var doc = System.Text.Json.JsonDocument.Parse(body);
            var accessToken = doc.RootElement
                .GetProperty("access_token")
                .GetString();

            return accessToken
                ?? throw new InvalidOperationException(
                    "access_token was null in token response");
        }


        protected async Task<(string AccessToken, string RefreshToken)> DoRefreshTokenAsync(
            string refreshToken, string? tenantId = null)
        {
            var http = _factory.CreateClient();
            if (!string.IsNullOrEmpty(tenantId))
                http.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId);

            var parameters = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken,
                ["client_id"] = "customer-management-swagger"
            };

            var response = await http.PostAsync(
                "connect/token",
                new FormUrlEncodedContent(parameters));

            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException(
                    $"Refresh token request failed ({(int)response.StatusCode}): {body}");

            var doc = System.Text.Json.JsonDocument.Parse(body);
            var newAccess = doc.RootElement.GetProperty("access_token").GetString()!;
            var newRefresh = doc.RootElement.GetProperty("refresh_token").GetString()!;

            return (newAccess, newRefresh);
        }

        protected async Task<string> GetRefreshedAccessTokenAsync(string refreshToken)
            => (await DoRefreshTokenAsync(refreshToken)).AccessToken;

        //  CRUD  

        protected static string UniqueMobile()
            => "01" + Random.Shared.Next(100000000, 999999999).ToString();

        protected async Task<CustomerDto> CreateTestCustomerAsync(
            Client authClient,
            string? name = "Test Customer",
            string? mobile = null,
            List<CreateAddressReq>? addresses = null)
        {
            return await authClient.Add2Async(new CreateCustomerReq
            {
                Name = name ?? "Test Customer",
                Mobile = mobile ?? UniqueMobile(),
                Adresses = addresses ?? new List<CreateAddressReq>()
            });
        }

        protected async Task<AddressDto> AddAddressAsync(
            Client authClient, Guid customerId, int? type = 1)
            => await authClient.AddAsync(customerId, new AddAddressReq
            {
                Type = type ?? 1,
                Value = "Secondary Address"
            });

        protected Task<ICollection<CustomerDto>> GetCustomersAsync(
            Client client, Guid? customerId = null)
            => client.Get2Async(customerId);

        protected Task<ICollection<AddressDto>> GetAddressesAsync(
            Client client, Guid? customerId = null, Guid? addressId = null)
            => client.GetAsync(customerId, addressId);

        protected async Task CleanupCustomerAsync(Client adminClient, Guid customerId)
        {
            try { await adminClient.Delete2Async(customerId); }
            catch { /* already deleted or never existed */ }
        }
    }
}