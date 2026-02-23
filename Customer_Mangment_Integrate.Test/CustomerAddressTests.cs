using Customer_Mangment;
using Customer_Mangment_Integrate.Test.Common;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Customer_Mangment_Integrate.Test
{
    public class CustomerAddressTests : TestBase
    {
        public CustomerAddressTests(WebApplicationFactory<IAssmblyMarker> factory) : base(factory) { }

        // ----- Add Address -----

        [Fact]
        public async Task AddAddress_ValidData_ReturnsAddressDto()
        {
            var token = await GetAdminTokenAsync();
            var client = CreateApiClient(token);

            var customer = await CreateTestCustomerAsync(client, "Address Customer", "01011110001");

            var address = await client.AddAsync(customer.Id, new AddAddressReq
            {
                Type = 2,
                Value = "New Cairo, 5th Settlement"
            });

            Assert.NotEqual(Guid.Empty, address.Id);
            Assert.Equal(customer.Id, address.CustomerId);
            Assert.Equal(2, address.Type);
            Assert.Equal("New Cairo, 5th Settlement", address.Value);
        }

        [Fact]
        public async Task AddAddress_AppearsInCustomerAddresses()
        {
            var token = await GetAdminTokenAsync();
            var client = CreateApiClient(token);

            var customer = await CreateTestCustomerAsync(client, "Verify Address");

            var address = await client.AddAsync(customer.Id, new AddAddressReq
            {
                Type = 2,
                Value = "Alexandria, Smouha"
            });

            var updated = (await client.GetAsync(customer.Id)).First();
            Assert.Contains(updated.Addresses, a => a.Id == address.Id);
        }

        [Fact]
        public async Task AddAddress_NonExistentCustomer_ThrowsNotFound()
        {
            var token = await GetAdminTokenAsync();
            var client = CreateApiClient(token);

            var ex = await Assert.ThrowsAsync<ApiException<ProblemDetails>>(
                () => client.AddAsync(Guid.NewGuid(), new AddAddressReq
                {
                    Type = 1,
                    Value = "Some Address"
                }));

            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task AddAddress_WithoutToken_ThrowsUnauthorized()
        {
            var client = CreateApiClient();

            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => client.AddAsync(Guid.NewGuid(), new AddAddressReq
                {
                    Type = 1,
                    Value = "Test"
                }));

            Assert.Equal(401, ex.StatusCode);
        }

        // ----- Update Address -----

        [Fact]
        public async Task UpdateAddress_ValidData_UpdatesSuccessfully()
        {
            var token = await GetAdminTokenAsync();
            var client = CreateApiClient(token);
            try
            {
                var customer = await CreateTestCustomerAsync(client, "Update Addr Customer");

                var address = await client.AddAsync(customer.Id, new AddAddressReq
                {
                    Type = 1,
                    Value = "Old Address"
                });

                await client.UpdateAsync(new UpdateAddressReq
                {
                    AddressId = address.Id,
                    Type = 2,
                    Value = "Updated Address"
                });

                var updated = (await client.GetAsync(customer.Id)).First();
                var updatedAddr = updated.Addresses.FirstOrDefault(a => a.Id == address.Id);
                Assert.NotNull(updatedAddr);
                Assert.Equal(2, updatedAddr.Type);
                Assert.Equal("Updated Address", updatedAddr.Value);
            }
            catch (ApiException ex)
            {
                throw new Exception($"AddAsync failed: Status={ex.StatusCode}, Detail={ex.Message}, Response={ex.Response}");
            }

        }

        [Fact]
        public async Task UpdateAddress_NonExistentAddressId_ThrowsNotFound()
        {
            var token = await GetAdminTokenAsync();
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

            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => client.UpdateAsync(new UpdateAddressReq
                {
                    AddressId = Guid.NewGuid(),
                    Type = 1,
                    Value = "Test"
                }));

            Assert.Equal(401, ex.StatusCode);
        }

        // ----- Delete Address -----

        [Fact]
        public async Task DeleteAddress_ValidId_DeletesSuccessfully()
        {
            var token = await GetAdminTokenAsync();
            var client = CreateApiClient(token);

            var customer = await CreateTestCustomerAsync(client, "Delete Addr Customer");
            var address = await client.AddAsync(customer.Id, new AddAddressReq
            {
                Type = 1,
                Value = "Address To Delete"
            });

            await client.DeleteAsync(address.Id);

            var updated = (await client.GetAsync(customer.Id)).First();
            Assert.DoesNotContain(updated.Addresses, a => a.Id == address.Id);
        }

        [Fact]
        public async Task DeleteAddress_NonExistentId_ThrowsNotFound()
        {
            var token = await GetAdminTokenAsync();
            var client = CreateApiClient(token);

            var ex = await Assert.ThrowsAsync<ApiException<ProblemDetails>>(
                () => client.DeleteAsync(Guid.NewGuid()));

            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task DeleteAddress_WithoutToken_ThrowsUnauthorized()
        {
            var client = CreateApiClient();

            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => client.DeleteAsync(Guid.NewGuid()));

            Assert.Equal(401, ex.StatusCode);
        }
    }

}
