using Customer_Mangment;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Headers;

namespace Customer_Mangment_Integrate.Test.Common
{
    public class TestBase(WebApplicationFactory<IAssmblyMarker> factory) : IClassFixture<WebApplicationFactory<IAssmblyMarker>>
    {
        protected readonly WebApplicationFactory<IAssmblyMarker> _factory = factory;


        protected Client CreateApiClient()
        {
            var httpClient = _factory.CreateClient();
            var client = new Client(httpClient);
            client.BaseUrl = httpClient.BaseAddress?.ToString() ?? "https://localhost:7063/";
            return client;
        }

        protected Client CreateApiClient(string accessToken)
        {
            var httpClient = _factory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);
            var client = new Client(httpClient);
            client.BaseUrl = httpClient.BaseAddress?.ToString() ?? "https://localhost:7063/";
            return client;
        }

        protected async Task<string> GetAdminTokenAsync()
        {
            var client = CreateApiClient();
            var response = await client.GenerateTokenAsync(new GenerateTokenQuery
            {
                Email = "admin@test.com",
                Password = "Admin@123"
            });
            return response.AccessToken;
        }

        protected async Task<string> GetUserTokenAsync()
        {
            var client = CreateApiClient();
            var response = await client.GenerateTokenAsync(new GenerateTokenQuery
            {
                Email = "user@test.com",
                Password = "User@123"
            });
            return response.AccessToken;
        }

        protected async Task<CustomerDto> CreateTestCustomerAsync(
            Client authClient,
            string name = "Test Customer",
            string? mobile = null)
        {
            var uniqueMobile = mobile ?? $"010{new Random().Next(10000000, 99999999)}";

            return await authClient.Add2Async(new CreateCustomerReq
            {
                Name = name,
                Mobile = uniqueMobile,
                Adresses = new List<CreateAddressReq>
        {
            new CreateAddressReq { Type = 1, Value = "123 Test St, Cairo" }
        }
            });
        }
    }

}
