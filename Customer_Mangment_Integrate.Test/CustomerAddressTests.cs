using Customer_Mangment;
using Customer_Mangment_Integrate.Test.Common;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Customer_Mangment_Integrate.Test
{
    public class CustomerAddressTests : TestBase
    {
        public CustomerAddressTests(WebApplicationFactory<IAssmblyMarker> factory) : base(factory) { }

        //Add

        [Fact]
        public async Task AddAddress_Admin_ReturnsAddressDto()
        {
            var client = CreateApiClient(await GetAdminTokenAsync());
            var customer = await CreateTestCustomerAsync(client);
            try
            {
                var address = await client.AddAsync(customer.Id, new AddAddressReq
                {
                    Type = 1,
                    Value = "New Cairo"
                });

                Assert.NotEqual(Guid.Empty, address.Id);
                Assert.Equal(customer.Id, address.CustomerId);
                Assert.Equal("New Cairo", address.Value);
            }
            finally { await CleanupCustomerAsync(client, customer.Id); }
        }

        [Fact]
        public async Task AddAddress_ReturnsCorrectType()
        {
            var client = CreateApiClient(await GetAdminTokenAsync());
            var customer = await CreateTestCustomerAsync(client);
            try
            {
                var address = await client.AddAsync(customer.Id, new AddAddressReq
                {
                    Type = 2,
                    Value = "Type 2 Address"
                });

                Assert.Equal(2, address.Type);
            }
            finally { await CleanupCustomerAsync(client, customer.Id); }
        }

        [Fact]
        public async Task AddAddress_AppearsInCustomerAddressList()
        {
            var client = CreateApiClient(await GetAdminTokenAsync());
            var customer = await CreateTestCustomerAsync(client);
            try
            {
                var address = await client.AddAsync(customer.Id, new AddAddressReq
                {
                    Type = 1,
                    Value = "Verify Address"
                });

                var updated = (await client.Get2Async(null, null));
                Assert.Contains(updated, a => a.Id == address.Id);
            }
            finally { await CleanupCustomerAsync(client, customer.Id); }
        }

        [Fact]
        public async Task AddAddress_IncreasesAddressCountByOne()
        {
            var client = CreateApiClient(await GetAdminTokenAsync());
            var customer = await CreateTestCustomerAsync(client);
            try
            {
                var before = (await client.Get2Async(customer.Id, null)).Count;

                await AddAddressAsync(client, customer.Id);

                var after = (await client.Get2Async(customer.Id, null)).Count;
                Assert.Equal(before + 1, after);
            }
            finally { await CleanupCustomerAsync(client, customer.Id); }
        }

        [Fact]
        public async Task AddAddress_AddressIdIsNotEmpty()
        {
            var client = CreateApiClient(await GetAdminTokenAsync());
            var customer = await CreateTestCustomerAsync(client);
            try
            {
                var address = await AddAddressAsync(client, customer.Id);
                Assert.NotEqual(Guid.Empty, address.Id);
            }
            finally { await CleanupCustomerAsync(client, customer.Id); }
        }

        [Fact]
        public async Task AddAddress_NonExistentCustomer_ThrowsNotFound()
        {
            var client = CreateApiClient(await GetAdminTokenAsync());

            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => client.AddAsync(Guid.NewGuid(), new AddAddressReq
                {
                    Type = 1,
                    Value = "Ghost"
                }));

            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task AddAddress_WithoutToken_ThrowsUnauthorized()
        {
            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => CreateApiClient().AddAsync(Guid.NewGuid(), new AddAddressReq
                {
                    Type = 1,
                    Value = "Test"
                }));

            Assert.Equal(401, ex.StatusCode);
        }

        [Fact]
        public async Task AddAddress_User_ThrowsForbiddenOrUnauthorized()
        {
            var admin = CreateApiClient(await GetAdminTokenAsync());
            var customer = await CreateTestCustomerAsync(admin);
            try
            {
                var user = CreateApiClient(await GetUserTokenAsync());

                var ex = await Assert.ThrowsAnyAsync<ApiException>(
                    () => user.AddAsync(customer.Id, new AddAddressReq
                    {
                        Type = 1,
                        Value = "Unauthorized"
                    }));

                Assert.True(ex.StatusCode == 401 || ex.StatusCode == 403);
            }
            finally { await CleanupCustomerAsync(admin, customer.Id); }
        }

        [Fact]
        public async Task GetHistory_AfterAddAddress_ContainsAddressHistory()
        {
            var client = CreateApiClient(await GetAdminTokenAsync());
            var customer = await CreateTestCustomerAsync(client);
            try
            {
                await AddAddressAsync(client, customer.Id);

                var history = await client.HistoryAsync(customer.Id);
                Assert.NotNull(history);
            }
            finally { await CleanupCustomerAsync(client, customer.Id); }
        }

        // ── Update ───────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAddress_Admin_UpdatesValue()
        {
            var client = CreateApiClient(await GetAdminTokenAsync());
            var customer = await CreateTestCustomerAsync(client);
            try
            {
                var address = await AddAddressAsync(client, customer.Id);

                await client.UpdateAsync(address.Id, new UpdateAddressReq
                {
                    Type = 1,
                    Value = "Updated Address Value"
                });

                var updatedAddr = (await client.Get2Async(null, null)).First(a => a.Id == address.Id);
                Assert.Equal("Updated Address Value", updatedAddr.Value);
            }
            finally { await CleanupCustomerAsync(client, customer.Id); }
        }

        [Fact]
        public async Task UpdateAddress_Admin_UpdatesType()
        {
            var client = CreateApiClient(await GetAdminTokenAsync());
            var customer = await CreateTestCustomerAsync(client);
            try
            {
                var address = await AddAddressAsync(client, customer.Id);

                await client.UpdateAsync(address.Id, new UpdateAddressReq
                {
                    Type = 2,
                    Value = address.Value
                });

                var updatedAddr = (await client.Get2Async(null, null)).First(a => a.Id == address.Id);

                Assert.Equal(2, updatedAddr.Type);
            }
            finally { await CleanupCustomerAsync(client, customer.Id); }
        }

        [Fact]
        public async Task UpdateAddress_NonExistentId_ThrowsNotFound()
        {
            var client = CreateApiClient(await GetAdminTokenAsync());
            var AddressId = Guid.NewGuid();
            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => client.UpdateAsync(AddressId, new UpdateAddressReq
                {
                    Type = 1,
                    Value = "Does not matter"
                }));

            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task UpdateAddress_WithoutToken_ThrowsUnauthorized()
        {
            var AddressId = Guid.NewGuid();

            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => CreateApiClient().UpdateAsync(AddressId, new UpdateAddressReq
                {
                    Type = 1,
                    Value = "Test"
                }));

            Assert.Equal(401, ex.StatusCode);
        }

        [Fact]
        public async Task UpdateAddress_User_ThrowsForbiddenOrUnauthorized()
        {
            var admin = CreateApiClient(await GetAdminTokenAsync());
            var customer = await CreateTestCustomerAsync(admin);
            try
            {
                var address = await AddAddressAsync(admin, customer.Id);
                var user = CreateApiClient(await GetUserTokenAsync());

                var ex = await Assert.ThrowsAnyAsync<ApiException>(
                    () => user.UpdateAsync(address.Id, new UpdateAddressReq
                    {
                        Type = 1,
                        Value = "Unauthorized"
                    }));

                Assert.True(ex.StatusCode == 401 || ex.StatusCode == 403);
            }
            finally { await CleanupCustomerAsync(admin, customer.Id); }
        }

        // ── Delete ───────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteAddress_Admin_DeletesSuccessfully()
        {
            var client = CreateApiClient(await GetAdminTokenAsync());
            var customer = await CreateTestCustomerAsync(client);
            try
            {
                var address = await AddAddressAsync(client, customer.Id);

                await client.DeleteAsync(address.Id);

                var updated = (await client.GetAsync(customer.Id)).First();
                Assert.DoesNotContain(updated.Addresses, a => a.Id == address.Id);
            }
            finally { await CleanupCustomerAsync(client, customer.Id); }
        }

        [Fact]
        public async Task DeleteAddress_DecreasesAddressCountByOne()
        {
            var client = CreateApiClient(await GetAdminTokenAsync());
            var customer = await CreateTestCustomerAsync(client);
            try
            {
                var address = await AddAddressAsync(client, customer.Id);
                var before = (await client.Get2Async(customer.Id, null)).Count;

                await client.DeleteAsync(address.Id);

                var after = (await client.Get2Async(customer.Id, null)).Count;
                Assert.Equal(before - 1, after);
            }
            finally { await CleanupCustomerAsync(client, customer.Id); }
        }

        [Fact]
        public async Task DeleteAddress_NonExistentId_ThrowsNotFound()
        {
            var client = CreateApiClient(await GetAdminTokenAsync());

            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => client.DeleteAsync(Guid.NewGuid()));

            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task DeleteAddress_WithoutToken_ThrowsUnauthorized()
        {
            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => CreateApiClient().DeleteAsync(Guid.NewGuid()));

            Assert.Equal(401, ex.StatusCode);
        }

        [Fact]
        public async Task DeleteAddress_User_ThrowsForbiddenOrUnauthorized()
        {
            var admin = CreateApiClient(await GetAdminTokenAsync());
            var customer = await CreateTestCustomerAsync(admin);
            try
            {
                var address = await AddAddressAsync(admin, customer.Id);
                var user = CreateApiClient(await GetUserTokenAsync());

                var ex = await Assert.ThrowsAnyAsync<ApiException>(
                    () => user.DeleteAsync(address.Id));

                Assert.True(ex.StatusCode == 401 || ex.StatusCode == 403);
            }
            finally { await CleanupCustomerAsync(admin, customer.Id); }
        }

        [Fact]
        public async Task DeleteAddress_Twice_SecondThrowsNotFound()
        {
            var client = CreateApiClient(await GetAdminTokenAsync());
            var customer = await CreateTestCustomerAsync(client);
            try
            {
                var address = await AddAddressAsync(client, customer.Id);

                await client.DeleteAsync(address.Id);

                var ex = await Assert.ThrowsAnyAsync<ApiException>(
                    () => client.DeleteAsync(address.Id));

                Assert.Equal(404, ex.StatusCode);
            }
            finally { await CleanupCustomerAsync(client, customer.Id); }
        }
    }

}
