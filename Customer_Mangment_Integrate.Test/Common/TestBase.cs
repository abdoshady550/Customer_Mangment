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

        public TestBase(WebApplicationFactory<IAssmblyMarker> factory) => _factory = factory;

        // Client

        protected Client CreateApiClient()
        {
            var http = CreateHttpClientWithTenant();
            return new Client(http) { BaseUrl = http.BaseAddress?.ToString() ?? "https://localhost:7063/" };
        }

        protected Client CreateApiClient(string accessToken)
        {
            var http = CreateHttpClientWithTenant(accessToken);
            return new Client(http) { BaseUrl = http.BaseAddress?.ToString() ?? "https://localhost:7063/" };
        }

        private HttpClient CreateHttpClientWithTenant(string? accessToken = null)
        {
            var http = _factory.CreateClient();
            http.DefaultRequestHeaders.Add("X-Tenant-Id", DefaultTenantId);
            if (!string.IsNullOrEmpty(accessToken))
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            return http;
        }
        protected async Task<string> GetAdminTokenAsync()
        {
            var http = _factory.CreateClient();
            http.DefaultRequestHeaders.Add("X-Tenant-Id", DefaultTenantId); // ← add this
            var client = new Client(http) { BaseUrl = http.BaseAddress?.ToString() ?? "https://localhost:7063/" };
            var response = await client.GenerateTokenAsync(new GenerateTokenQuery
            {
                Email = AdminEmail,
                Password = AdminPassword
            });
            return response.AccessToken;
        }

        protected async Task<string> GetUserTokenAsync()
        {
            var http = _factory.CreateClient();
            http.DefaultRequestHeaders.Add("X-Tenant-Id", DefaultTenantId); // ← add this
            var client = new Client(http) { BaseUrl = http.BaseAddress?.ToString() ?? "https://localhost:7063/" };
            var response = await client.GenerateTokenAsync(new GenerateTokenQuery
            {
                Email = UserEmail,
                Password = UserPassword
            });
            return response.AccessToken;
        }

        protected static string UniqueMobile()
            => "01" + Random.Shared.Next(100000000, 999999999).ToString();

        // ── CRUD Helpers ──────────────────────────────────────────────────

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

        protected async Task<AddressDto> AddAddressAsync(Client authClient, Guid customerId, int? type = 1)
            => await authClient.AddAsync(customerId, new AddAddressReq
            {
                Type = type ?? 1,
                Value = "Secondary Address"
            });

        protected Task<ICollection<CustomerDto>> GetCustomersAsync(Client client, Guid? customerId = null)
            => client.Get2Async(customerId);

        protected Task<ICollection<AddressDto>> GetAddressesAsync(Client client, Guid? customerId = null, Guid? addressId = null)
            => client.GetAsync(customerId, addressId);

        protected async Task CleanupCustomerAsync(Client adminClient, Guid customerId)
        {
            try { await adminClient.Delete2Async(customerId); }
            catch { /* already deleted or never existed */ }
        }
    }
}