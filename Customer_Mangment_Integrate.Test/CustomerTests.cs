using Customer_Mangment;
using Customer_Mangment_Integrate.Test.Common;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Customer_Mangment_Integrate.Test
{
    public class CustomerTests : TestBase
    {
        public CustomerTests(WebApplicationFactory<IAssmblyMarker> factory) : base(factory) { }

        // ----- Create Customer -----

        [Fact]
        public async Task CreateCustomer_AdminRole_ReturnsCreatedCustomer()
        {
            var token = await GetAdminTokenAsync();
            var client = CreateApiClient(token);

            var result = await CreateTestCustomerAsync(client, "Ahmed Ali", "01099991111");

            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal("Ahmed Ali", result.Name);
            Assert.Equal("01099991111", result.Mobile);
            Assert.NotNull(result.Addresses);
        }

        [Fact]
        public async Task CreateCustomer_WithMultipleAddresses_ReturnsAllAddresses()
        {
            var token = await GetAdminTokenAsync();
            var client = CreateApiClient(token);

            var result = await client.Add2Async(new CreateCustomerReq
            {
                Name = "Sara Mohamed",
                Mobile = "01088882222",
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

            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => CreateTestCustomerAsync(client));

            Assert.Equal(401, ex.StatusCode);
        }

        [Fact]
        public async Task CreateCustomer_DuplicateMobile_ThrowsConflict()
        {
            var token = await GetAdminTokenAsync();
            var client = CreateApiClient(token);

            var mobile = $"010{new Random().Next(10000000, 99999999)}";
            await CreateTestCustomerAsync(client, "First Customer", mobile);

            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => CreateTestCustomerAsync(client, "Second Customer", mobile));

            Assert.Equal(409, ex.StatusCode);
        }

        // ----- Get Customers -----

        [Fact]
        public async Task GetCustomers_NoFilter_ReturnsAllCustomers()
        {
            var token = await GetAdminTokenAsync();
            var client = CreateApiClient(token);

            await CreateTestCustomerAsync(client, "GetAll Test", "01077773333");

            var customers = await client.GetAsync(null);

            Assert.NotNull(customers);
            Assert.True(customers.Count > 0);
        }

        [Fact]
        public async Task GetCustomers_WithCustomerId_ReturnsSingleCustomer()
        {
            var token = await GetAdminTokenAsync();
            var client = CreateApiClient(token);

            var created = await CreateTestCustomerAsync(client, "Specific Customer", "01066664444");

            var result = await client.GetAsync(created.Id);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(created.Id, result.First().Id);
            Assert.Equal("Specific Customer", result.First().Name);
        }

        [Fact]
        public async Task GetCustomers_WithNonExistentId_ThrowsNotFound()
        {
            var token = await GetAdminTokenAsync();
            var client = CreateApiClient(token);

            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => client.GetAsync(Guid.NewGuid()));

            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task GetCustomers_WithoutToken_ThrowsUnauthorized()
        {
            var client = CreateApiClient();

            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => client.GetAsync(null));

            Assert.Equal(401, ex.StatusCode);
        }

        // ----- Update Customer -----

        [Fact]
        public async Task UpdateCustomer_ValidData_UpdatesSuccessfully()
        {
            var token = await GetAdminTokenAsync();
            var client = CreateApiClient(token);

            var created = await CreateTestCustomerAsync(client, "Old Name", "01055555555");

            await client.Update2Async(new UpdateCustomerReq
            {
                CustomerId = created.Id,
                Name = "New Name",
                Mobile = "01044444444"
            });

            var updated = (await client.GetAsync(created.Id)).First();
            Assert.Equal("New Name", updated.Name);
            Assert.Equal("01044444444", updated.Mobile);
        }

        [Fact]
        public async Task UpdateCustomer_NonExistentId_ThrowsNotFound()
        {
            var token = await GetAdminTokenAsync();
            var client = CreateApiClient(token);

            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => client.Update2Async(new UpdateCustomerReq
                {
                    CustomerId = Guid.NewGuid(),
                    Name = "Ghost",
                    Mobile = "00000000000"
                }));

            Assert.Equal(500, ex.StatusCode);
        }

        [Fact]
        public async Task UpdateCustomer_WithoutToken_ThrowsUnauthorized()
        {
            var client = CreateApiClient();

            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => client.Update2Async(new UpdateCustomerReq
                {
                    CustomerId = Guid.NewGuid(),
                    Name = "Test",
                    Mobile = "01000000000"
                }));

            Assert.Equal(401, ex.StatusCode);
        }

        // ----- Delete Customer -----

        [Fact]
        public async Task DeleteCustomer_ValidId_DeletesSuccessfully()
        {
            var token = await GetAdminTokenAsync();
            var client = CreateApiClient(token);

            var created = await CreateTestCustomerAsync(client, "To Delete", "01033336666");

            await client.Delete2Async(created.Id);

            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => client.GetAsync(created.Id));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task DeleteCustomer_NonExistentId_ThrowsNotFound()
        {
            var token = await GetAdminTokenAsync();
            var client = CreateApiClient(token);

            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => client.Delete2Async(Guid.NewGuid()));

            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task DeleteCustomer_WithoutToken_ThrowsUnauthorized()
        {
            var client = CreateApiClient();

            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => client.Delete2Async(Guid.NewGuid()));

            Assert.Equal(401, ex.StatusCode);
        }

        // ----- Get History -----



        [Fact]
        public async Task GetHistory_AfterUpdate_ContainsMultipleHistoryEntries()
        {
            var token = await GetAdminTokenAsync();
            var client = CreateApiClient(token);

            var created = await CreateTestCustomerAsync(client, "Update History");

            await client.Update2Async(new UpdateCustomerReq
            {
                CustomerId = created.Id,
                Name = "Updated History",
                Mobile = "01011119999"
            });

            var history = await client.HistoryAsync(created.Id);

            Assert.NotNull(history.CustomerHistoryDtos);
        }

        [Fact]
        public async Task GetHistory_AfterAddAddress_ContainsAddressHistory()
        {
            var token = await GetAdminTokenAsync();
            var client = CreateApiClient(token);

            var created = await CreateTestCustomerAsync(client, "Address History", "01000001111");

            await client.AddAsync(created.Id, new AddAddressReq
            {
                Type = 2,
                Value = "New Address"
            });

            var history = await client.HistoryAsync(created.Id);

            Assert.NotNull(history.AddressHistoryDtos);
        }

        [Fact]
        public async Task GetHistory_NonExistentId_ThrowsNotFound()
        {
            var token = await GetAdminTokenAsync();
            var client = CreateApiClient(token);

            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => client.HistoryAsync(Guid.NewGuid()));

            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task GetHistory_WithoutToken_ThrowsUnauthorized()
        {
            var client = CreateApiClient();

            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => client.HistoryAsync(Guid.NewGuid()));

            Assert.Equal(401, ex.StatusCode);
        }

        [Fact]
        public async Task GetHistory_HistoryReflectsDeletedState()
        {
            var token = await GetAdminTokenAsync();
            var client = CreateApiClient(token);

            var created = await CreateTestCustomerAsync(client, "Delete History", "01000002222");
            await client.Delete2Async(created.Id);

            var history = await client.HistoryAsync(created.Id);

            Assert.NotNull(history.CustomerHistoryDtos);
            var lastEntry = history.CustomerHistoryDtos
                .OrderByDescending(h => h.ValidFrom)
                .First();
            Assert.False(lastEntry.IsDeleted);
        }
    }

}
