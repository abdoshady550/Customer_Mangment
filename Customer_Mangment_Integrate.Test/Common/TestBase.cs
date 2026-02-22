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
            var apiClient = new Client(httpClient);


            apiClient.BaseUrl = httpClient.BaseAddress?.ToString() ?? "https://localhost:7063/";

            return apiClient;
        }

        protected Client CreateApiClient(string accessToken)
        {
            var httpClient = _factory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var apiClient = new Client(httpClient);
            apiClient.BaseUrl = httpClient.BaseAddress?.ToString() ?? "https://localhost:7063/";

            return apiClient;
        }


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

        protected async Task<CustomerDto> CreateTestCustomerAsync(
            Client authClient,
            string name = "Test Customer",
            string mobile = "01012345678")
        {
            return await authClient.CustomerPOSTAsync(new CreateCustomerReq
            {
                Name = name,
                Mobile = mobile,
                Adresses = new List<CreateAddressReq>
                {
                    new CreateAddressReq { Type = 1, Value = "123 Test Street, Cairo" }
                }
            });
        }
    }

}
