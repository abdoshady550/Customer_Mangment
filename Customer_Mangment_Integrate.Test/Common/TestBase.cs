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

        public TestBase(WebApplicationFactory<IAssmblyMarker> factory) => _factory = factory;

        // Client

        protected Client CreateApiClient()
        {
            var http = _factory.CreateClient();
            return new Client(http) { BaseUrl = http.BaseAddress?.ToString() ?? "https://localhost:7063/" };
        }

        protected Client CreateApiClient(string accessToken)
        {
            var http = _factory.CreateClient();
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            return new Client(http) { BaseUrl = http.BaseAddress?.ToString() ?? "https://localhost:7063/" };
        }

        // Tokens
        protected async Task<string> GetTokenAsync(
            string email = "admin@test.com",
            string password = "Test@123")
        {
            var client = CreateApiClient();
            var response = await client.GenerateTokenAsync(new GenerateTokenQuery
            {
                Email = email,
                Password = password
            });
            return response.AccessToken;
        }
        protected async Task<string> GetAdminTokenAsync()
        {
            var client = CreateApiClient();
            var response = await client.GenerateTokenAsync(new GenerateTokenQuery
            {
                Email = AdminEmail,
                Password = AdminPassword
            });
            return response.AccessToken;
        }

        protected async Task<string> GetUserTokenAsync()
        {
            var client = CreateApiClient();
            var response = await client.GenerateTokenAsync(new GenerateTokenQuery
            {
                Email = UserEmail,
                Password = UserPassword
            });
            return response.AccessToken;
        }


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
                Adresses = addresses ?? null

            });
        }

        protected async Task<AddressDto> AddAddressAsync(Client authClient, Guid customerId, int? type = 1)
            => await authClient.AddAsync(customerId, new AddAddressReq
            {
                Type = type ?? 1,
                Value = "Secondary Address"
            });
        protected async Task CleanupCustomerAsync(Client adminClient, Guid customerId)
        {
            try { await adminClient.Delete2Async(customerId); }
            catch { /* already deleted or never existed */ }
        }
    }

}
