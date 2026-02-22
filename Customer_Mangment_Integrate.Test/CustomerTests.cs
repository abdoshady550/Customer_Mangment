using Customer_Mangment;
using Customer_Mangment_Integrate.Test.Common;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Customer_Mangment_Integrate.Test
{
    public class CustomerTests : TestBase
    {
        public CustomerTests(WebApplicationFactory<IAssmblyMarker> factory) : base(factory) { }

        [Fact]
        public async Task CreateCustomer_ValidData_ReturnsCreatedCustomer()
        {
            var token = await GetTokenAsync();
            var client = CreateApiClient(token);

            var result = await CreateTestCustomerAsync(client, "Ahmed Ali", "01099999999");

            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal("Ahmed Ali", result.Name);
            Assert.Equal("01099999999", result.Mobile);
        }

        [Fact]
        public async Task CreateCustomer_WithAddresses_ReturnsCustomerWithAddresses()
        {
            var token = await GetTokenAsync();
            var client = CreateApiClient(token);

            var result = await client.CustomerPOSTAsync(new CreateCustomerReq
            {
                Name = "Sara Mohamed",
                Mobile = "01011111111",
                Adresses = new List<CreateAddressReq>
                {
                    new CreateAddressReq { Type = 1, Value = "Cairo, Nasr City" },
                    new CreateAddressReq { Type = 2, Value = "Giza, Dokki" }
                }
            });

            Assert.NotNull(result.Addresses);
            Assert.Equal(2, result.Addresses.Count);
        }

        [Fact]
        public async Task CreateCustomer_WithoutToken_ThrowsUnauthorized()
        {
            var client = CreateApiClient();

            var ex = await Assert.ThrowsAsync<ApiException<ProblemDetails>>(
                () => CreateTestCustomerAsync(client));

            Assert.Equal(401, ex.StatusCode);
        }
        [Fact]
        public async Task GetCustomers_NoFilter_ReturnsAllCustomers()
        {
            var token = await GetTokenAsync();
            var client = CreateApiClient(token);

            await CreateTestCustomerAsync(client, "GetAll Test", "01022222222");

            var customers = await client.CustomerAllAsync(null);

            Assert.NotNull(customers);
            Assert.True(customers.Count > 0);
        }

        [Fact]
        public async Task GetCustomers_WithCustomerId_ReturnsSingleCustomer()
        {
            var token = await GetTokenAsync();
            var client = CreateApiClient(token);

            var created = await CreateTestCustomerAsync(client, "Specific Customer", "01033333333");

            var result = await client.CustomerAllAsync(created.Id);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(created.Id, result.First().Id);
            Assert.Equal("Specific Customer", result.First().Name);
        }

        [Fact]
        public async Task GetCustomers_WithNonExistentId_ThrowsNotFound()
        {
            var token = await GetTokenAsync();
            var client = CreateApiClient(token);

            var ex = await Assert.ThrowsAsync<ApiException<ProblemDetails>>(
                () => client.CustomerAllAsync(Guid.NewGuid()));

            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task GetCustomers_WithoutToken_ThrowsUnauthorized()
        {
            var client = CreateApiClient();

            var ex = await Assert.ThrowsAsync<ApiException<ProblemDetails>>(
                () => client.CustomerAllAsync(null));

            Assert.Equal(401, ex.StatusCode);
        }

        [Fact]
        public async Task UpdateCustomer_ValidData_ReturnsUpdated()
        {
            var token = await GetTokenAsync();
            var client = CreateApiClient(token);

            var created = await CreateTestCustomerAsync(client, "Old Name", "01044444444");

            var result = await client.CustomerPUTAsync(new UpdateCustomerReq
            {
                CustomerId = created.Id,
                Name = "New Name",
                Mobile = "01055555555"
            });

            Assert.NotNull(result);

            var updated = await client.CustomerAllAsync(created.Id);
            Assert.Equal("New Name", updated.First().Name);
            Assert.Equal("01055555555", updated.First().Mobile);
        }

        [Fact]
        public async Task UpdateCustomer_NonExistentId_ThrowsNotFound()
        {
            var token = await GetTokenAsync();
            var client = CreateApiClient(token);

            var ex = await Assert.ThrowsAsync<ApiException<ProblemDetails>>(
                () => client.CustomerPUTAsync(new UpdateCustomerReq
                {
                    CustomerId = Guid.NewGuid(),
                    Name = "Ghost",
                    Mobile = "00000000000"
                }));

            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task UpdateCustomer_WithoutToken_ThrowsUnauthorized()
        {
            var client = CreateApiClient();

            var ex = await Assert.ThrowsAsync<ApiException<ProblemDetails>>(
                () => client.CustomerPUTAsync(new UpdateCustomerReq
                {
                    CustomerId = Guid.NewGuid(),
                    Name = "Test",
                    Mobile = "01000000000"
                }));

            Assert.Equal(401, ex.StatusCode);
        }

        [Fact]
        public async Task DeleteCustomer_ValidId_DeletesSuccessfully()
        {
            var token = await GetTokenAsync();
            var client = CreateApiClient(token);

            var created = await CreateTestCustomerAsync(client, "To Delete", "01066666666");

            var result = await client.CustomerDELETEAsync(created.Id);
            Assert.NotNull(result);

            var ex = await Assert.ThrowsAsync<ApiException<ProblemDetails>>(
                () => client.CustomerAllAsync(created.Id));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task DeleteCustomer_NonExistentId_ThrowsNotFound()
        {
            var token = await GetTokenAsync();
            var client = CreateApiClient(token);

            var ex = await Assert.ThrowsAsync<ApiException<ProblemDetails>>(
                () => client.CustomerDELETEAsync(Guid.NewGuid()));

            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task DeleteCustomer_WithoutToken_ThrowsUnauthorized()
        {
            var client = CreateApiClient();

            var ex = await Assert.ThrowsAsync<ApiException<ProblemDetails>>(
                () => client.CustomerDELETEAsync(Guid.NewGuid()));

            Assert.Equal(401, ex.StatusCode);
        }

        [Fact]
        public async Task GetCustomerHistory_AfterCreate_ContainsCreateEntry()
        {
            var token = await GetTokenAsync();
            var client = CreateApiClient(token);

            var created = await CreateTestCustomerAsync(client, "History Test", "01077777777");

            var history = await client.HistoryAsync(created.Id);

            Assert.NotNull(history);
            Assert.NotEmpty(history);
            Assert.Contains(history, h => h.Action.Contains("Create", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetCustomerHistory_AfterUpdate_ContainsUpdateEntry()
        {
            var token = await GetTokenAsync();
            var client = CreateApiClient(token);

            var created = await CreateTestCustomerAsync(client, "History Update", "01088888888");

            await client.CustomerPUTAsync(new UpdateCustomerReq
            {
                CustomerId = created.Id,
                Name = "History Updated",
                Mobile = "01099999998"
            });

            var history = await client.HistoryAsync(created.Id);

            Assert.True(history.Count >= 2);
            Assert.Contains(history, h => h.Action.Contains("Update", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetCustomerHistory_NonExistentId_ThrowsNotFound()
        {
            var token = await GetTokenAsync();
            var client = CreateApiClient(token);

            var ex = await Assert.ThrowsAsync<ApiException<ProblemDetails>>(
                () => client.HistoryAsync(Guid.NewGuid()));

            Assert.Equal(404, ex.StatusCode);
        }
    }

}
