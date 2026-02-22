using Customer_Mangment;
using Customer_Mangment_Integrate.Test.Common;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Customer_Mangment_Integrate.Test
{
    public class CustomerAddressTests : TestBase
    {
        public CustomerAddressTests(WebApplicationFactory<IAssmblyMarker> factory) : base(factory) { }

        [Fact]
        public async Task AddAddress_ValidData_ReturnsAddressDto()
        {
            var token = await GetTokenAsync();
            var client = CreateApiClient(token);

            var customer = await CreateTestCustomerAsync(client, "Address Customer", "01011110000");

            var address = await client.CustomerAddressPOSTAsync(new AddAddressReq
            {
                AddrssId = customer.Id,
                Type = 1,
                Value = "New Cairo, 5th Settlement"
            });

            Assert.NotEqual(Guid.Empty, address.Id);
            Assert.Equal(customer.Id, address.CustomerId);
            Assert.Equal(1, address.Type);
            Assert.Equal("New Cairo, 5th Settlement", address.Value);
        }

        [Fact]
        public async Task AddAddress_NonExistentCustomer_ThrowsNotFound()
        {
            var token = await GetTokenAsync();
            var client = CreateApiClient(token);

            var ex = await Assert.ThrowsAsync<ApiException<ProblemDetails>>(
                () => client.CustomerAddressPOSTAsync(new AddAddressReq
                {
                    AddrssId = Guid.NewGuid(),
                    Type = 1,
                    Value = "Some Address"
                }));

            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task AddAddress_WithoutToken_ThrowsUnauthorized()
        {
            var client = CreateApiClient();

            var ex = await Assert.ThrowsAsync<ApiException<ProblemDetails>>(
                () => client.CustomerAddressPOSTAsync(new AddAddressReq
                {
                    AddrssId = Guid.NewGuid(),
                    Type = 1,
                    Value = "Test"
                }));

            Assert.Equal(401, ex.StatusCode);
        }

        [Fact]
        public async Task UpdateAddress_ValidData_ReturnsUpdated()
        {
            var token = await GetTokenAsync();
            var client = CreateApiClient(token);

            var customer = await CreateTestCustomerAsync(client, "Update Address Customer", "01022220000");
            var address = await client.CustomerAddressPOSTAsync(new AddAddressReq
            {
                AddrssId = customer.Id,
                Type = 1,
                Value = "Old Address Value"
            });

            var result = await client.UpdateAsync(new UpdateAddressReq
            {
                AddressId = address.Id,
                Type = 2,
                Value = "Updated Address Value"
            });

            Assert.NotNull(result);

            var updated = (await client.CustomerAllAsync(customer.Id)).First();
            var updatedAddr = updated.Addresses.FirstOrDefault(a => a.Id == address.Id);
            Assert.NotNull(updatedAddr);
            Assert.Equal(2, updatedAddr.Type);
            Assert.Equal("Updated Address Value", updatedAddr.Value);
        }

        [Fact]
        public async Task UpdateAddress_NonExistentAddressId_ThrowsNotFound()
        {
            var token = await GetTokenAsync();
            var client = CreateApiClient(token);

            var ex = await Assert.ThrowsAsync<ApiException<ProblemDetails>>(
                () => client.UpdateAsync(new UpdateAddressReq
                {
                    AddressId = Guid.NewGuid(),
                    Type = 1,
                    Value = "Does not matter"
                }));

            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task UpdateAddress_WithoutToken_ThrowsUnauthorized()
        {
            var client = CreateApiClient();

            var ex = await Assert.ThrowsAsync<ApiException<ProblemDetails>>(
                () => client.UpdateAsync(new UpdateAddressReq
                {
                    AddressId = Guid.NewGuid(),
                    Type = 1,
                    Value = "Test"
                }));

            Assert.Equal(401, ex.StatusCode);
        }

        [Fact]
        public async Task DeleteAddress_ValidId_DeletesSuccessfully()
        {
            var token = await GetTokenAsync();
            var client = CreateApiClient(token);

            var customer = await CreateTestCustomerAsync(client, "Delete Address Customer", "01033330000");
            var address = await client.CustomerAddressPOSTAsync(new AddAddressReq
            {
                AddrssId = customer.Id,
                Type = 1,
                Value = "Address To Delete"
            });

            var result = await client.CustomerAddressDELETEAsync(address.Id);
            Assert.NotNull(result);

            var updated = (await client.CustomerAllAsync(customer.Id)).First();
            Assert.DoesNotContain(updated.Addresses, a => a.Id == address.Id);
        }

        [Fact]
        public async Task DeleteAddress_NonExistentId_ThrowsNotFound()
        {
            var token = await GetTokenAsync();
            var client = CreateApiClient(token);

            var ex = await Assert.ThrowsAsync<ApiException<ProblemDetails>>(
                () => client.CustomerAddressDELETEAsync(Guid.NewGuid()));

            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task DeleteAddress_WithoutToken_ThrowsUnauthorized()
        {
            var client = CreateApiClient();

            var ex = await Assert.ThrowsAsync<ApiException<ProblemDetails>>(
                () => client.CustomerAddressDELETEAsync(Guid.NewGuid()));

            Assert.Equal(401, ex.StatusCode);
        }
    }

}
