using System.Net.Http.Headers;

namespace Customer_Mangment_Integrate.Test.Common
{
    public class TestBase : IClassFixture<CustomWebApplicationFactory>
    {
        protected readonly CustomWebApplicationFactory _factory;

        protected const string AdminEmail = "admin@test.com";
        protected const string AdminPassword = "Admin@123";
        protected const string UserEmail = "user@test.com";
        protected const string UserPassword = "User@123";

        protected const string DefaultTenantId = "demo";

        public TestBase(CustomWebApplicationFactory factory) => _factory = factory;

        // ── IdentityClient factory ────────────────────────────────────────────

        /// <summary>Creates a typed IdentityClient backed by the in-process identity server.</summary>
        protected IdentityClient CreateIdentityClient(string? tenantId = null)
        {
            var http = _factory.CreateIdentityClient();
            if (!string.IsNullOrWhiteSpace(tenantId))
                http.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId);
            return new IdentityClient(http);
        }

        // ── API client factories ──────────────────────────────────────────────

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

        // ── Token helpers (now all go through IdentityClient) ─────────────────

        protected async Task<string> GetAdminTokenAsync()
        {
            var token = await CreateIdentityClient(DefaultTenantId)
                .GetPasswordTokenAsync(AdminEmail, AdminPassword);
            return token.AccessToken;
        }

        protected async Task<string> GetUserTokenAsync()
        {
            var token = await CreateIdentityClient(DefaultTenantId)
                .GetPasswordTokenAsync(UserEmail, UserPassword);
            return token.AccessToken;
        }

        /// <summary>Returns both access and refresh tokens for the given credentials.</summary>
        protected async Task<(string AccessToken, string RefreshToken)> GetTokenPairAsync(
            string email, string password, string? tenantId = null)
        {
            var token = await CreateIdentityClient(tenantId)
                .GetPasswordTokenAsync(email, password, tenantId: tenantId);
            return (token.AccessToken, token.RefreshToken);
        }

        /// <summary>Exchanges a refresh token for a new token pair.</summary>
        protected async Task<(string AccessToken, string RefreshToken)> DoRefreshTokenAsync(
            string refreshToken, string? tenantId = null)
        {
            var token = await CreateIdentityClient(tenantId)
                .RefreshTokenAsync(refreshToken, tenantId: tenantId);
            return (token.AccessToken, token.RefreshToken);
        }

        protected async Task<string> GetRefreshedAccessTokenAsync(string refreshToken)
            => (await DoRefreshTokenAsync(refreshToken)).AccessToken;

        // ── CRUD helpers ──────────────────────────────────────────────────────

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