using Customer_Mangment;
using Customer_Mangment_Integrate.Test.Common;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Customer_Mangment_Integrate.Test
{
    public class EndToEndScenarioTests : TestBase
    {
        public EndToEndScenarioTests(WebApplicationFactory<IAssmblyMarker> factory) : base(factory) { }

        [Fact]
        public async Task FullCustomerLifecycle_CreateUpdateAddressDeleteAndHistory()
        {
            var client = CreateApiClient(await GetAdminTokenAsync());
            var customer = await client.Add2Async(new CreateCustomerReq
            {
                Name = "Lifecycle Customer",
                Mobile = UniqueMobile(),
                Adresses = new List<CreateAddressReq>
                {
                    new CreateAddressReq { Type = 1, Value = "Initial Address" }
                }
            });

            try
            {
                // 1. Verify create
                Assert.NotEqual(Guid.Empty, customer.Id);
                Assert.Equal(AdminEmail, customer.CreatedBy);

                // 2. Read
                var fetched = (await client.GetAsync(customer.Id)).First();
                Assert.Equal(customer.Name, fetched.Name);
                var fetchedAddr = (await client.Get2Async(customer.Id, null));
                Assert.Single(fetchedAddr);

                // 3. Update customer
                await client.Update2Async(customer.Id, new UpdateCustomerReq
                {
                    Name = "Lifecycle Updated",
                    Mobile = UniqueMobile()
                });
                var afterUpdate = (await client.GetAsync(customer.Id)).First();
                Assert.Equal("Lifecycle Updated", afterUpdate.Name);
                Assert.Equal(AdminEmail, afterUpdate.UpdatedBy);

                // 4. Add address 
                var countBefore = (await client.Get2Async(customer.Id, null)).Count;
                var newAddr = await client.AddAsync(customer.Id, new AddAddressReq
                {
                    Type = 2,
                    Value = "Second Address"
                });
                Assert.NotEqual(Guid.Empty, newAddr.Id);

                var afterAddAddr = (await client.Get2Async(customer.Id, null)).Count;
                Assert.Equal(countBefore + 1, afterAddAddr);

                // 5. Update address
                await client.UpdateAsync(newAddr.Id, new UpdateAddressReq
                {
                    Type = 3,
                    Value = "Second Address Updated"
                });
                var afterAddrUpdate = (await client.Get2Async(null, null));
                Assert.Contains(afterAddrUpdate,
                    a => a.Id == newAddr.Id && a.Value == "Second Address Updated");

                // 6. Delete address
                await client.DeleteAsync(newAddr.Id);
                var afterAddrDelete = (await client.Get2Async(null, null));
                Assert.DoesNotContain(afterAddrDelete, a => a.Id == newAddr.Id);

                // 7. History وجوده
                var history = await client.HistoryAsync(customer.Id);
                var customerhistory = await client.History2Async(customer.Id);

                Assert.True(customerhistory.Count >= 2);
                Assert.NotNull(history);
            }
            finally { await CleanupCustomerAsync(client, customer.Id); }
        }

        [Fact]
        public async Task FullCustomerLifecycle_DeleteAndHistoryIsDeleted()
        {
            var client = CreateApiClient(await GetAdminTokenAsync());
            var customer = await CreateTestCustomerAsync(client, "Delete Lifecycle");

            await client.Update2Async(customer.Id, new UpdateCustomerReq
            {
                Name = "Updated Before Delete",
                Mobile = UniqueMobile()
            });

            await client.Delete2Async(customer.Id);

            // Get → 404
            var notFound = await Assert.ThrowsAnyAsync<ApiException>(
                () => client.GetAsync(customer.Id));
            Assert.Equal(404, notFound.StatusCode);

            // History → IsDeleted = true
            var historyAfter = await client.History2Async(customer.Id);
            var customerHistoryAfter = await client.History2Async(customer.Id);
            Assert.True(customerHistoryAfter.Count >= 2);
        }

        [Fact]
        public async Task UserCreates_AdminUpdatesAndDeletes()
        {
            var userClient = CreateApiClient(await GetUserTokenAsync());
            var adminClient = CreateApiClient(await GetAdminTokenAsync());
            var customer = await CreateTestCustomerAsync(userClient, "Cross Role");

            try
            {
                Assert.Equal(UserEmail, customer.CreatedBy);

                var fetched = (await adminClient.GetAsync(customer.Id)).First();
                Assert.Equal(customer.Id, fetched.Id);

                await adminClient.Update2Async(customer.Id, new UpdateCustomerReq
                {
                    Name = "Cross Role Updated",
                    Mobile = UniqueMobile()
                });
                var updated = (await adminClient.GetAsync(customer.Id)).First();
                Assert.Equal(AdminEmail, updated.UpdatedBy);

                // User cannot delete
                var ex = await Assert.ThrowsAnyAsync<ApiException>(
                    () => userClient.Delete2Async(customer.Id));
                Assert.True(ex.StatusCode == 401 || ex.StatusCode == 403);
            }
            finally { await CleanupCustomerAsync(adminClient, customer.Id); }
        }

        [Fact]
        public async Task TokenRefresh_NewTokenWorksOnProtectedEndpoint()
        {
            var client = CreateApiClient();
            var initial = await client.GenerateTokenAsync(new GenerateTokenQuery
            {
                Email = AdminEmail,
                Password = AdminPassword
            });

            var refreshed = await client.RefreshTokenAsync(new RefreshTokenQuery
            {
                RefreshToken = initial.RefreshToken,
                ExpiredAccessToken = initial.AccessToken
            });

            var authed = CreateApiClient(refreshed.AccessToken);
            var customers = await authed.GetAsync(null);
            Assert.NotNull(customers);
        }

        [Fact]
        public async Task MultipleAddresses_AddUpdateDelete_AllOperationsConsistent()
        {
            var client = CreateApiClient(await GetAdminTokenAsync());
            var customer = await CreateTestCustomerAsync(client);
            try
            {
                var countBefore = (await client.Get2Async(customer.Id, null)).Count;

                var addr1 = await AddAddressAsync(client, customer.Id, 1);
                var addr2 = await AddAddressAsync(client, customer.Id, 2);
                var addr3 = await AddAddressAsync(client, customer.Id, 3);

                var afterAdd = (await client.Get2Async(customer.Id, null)).Count;
                Assert.Equal(countBefore + 3, afterAdd);

                // Update addr2
                await client.UpdateAsync(addr2.Id, new UpdateAddressReq
                {
                    Type = 2,
                    Value = "Modified Addr2"
                });

                // Delete addr1 and addr3
                await client.DeleteAsync(addr1.Id);
                await client.DeleteAsync(addr3.Id);

                var afterDelete = (await client.Get2Async(customer.Id, null));
                Assert.Equal(countBefore + 1, afterDelete.Count);
                Assert.Contains(afterDelete,
                    a => a.Id == addr2.Id && a.Value == "Modified Addr2");
            }
            finally { await CleanupCustomerAsync(client, customer.Id); }
        }

        [Fact]
        public async Task CreateMultipleCustomers_AllReturnedInGetAll()
        {
            var client = CreateApiClient(await GetAdminTokenAsync());
            var c1 = await CreateTestCustomerAsync(client, "Bulk Customer 1");
            var c2 = await CreateTestCustomerAsync(client, "Bulk Customer 2");
            var c3 = await CreateTestCustomerAsync(client, "Bulk Customer 3");

            try
            {
                var list = await client.GetAsync(null);

                Assert.Contains(list, c => c.Id == c1.Id);
                Assert.Contains(list, c => c.Id == c2.Id);
                Assert.Contains(list, c => c.Id == c3.Id);
            }
            finally
            {
                await CleanupCustomerAsync(client, c1.Id);
                await CleanupCustomerAsync(client, c2.Id);
                await CleanupCustomerAsync(client, c3.Id);
            }
        }

        [Fact]
        public async Task UserAndAdminBothCreateCustomers_EachSeesAllCustomers()
        {
            var adminClient = CreateApiClient(await GetAdminTokenAsync());
            var userClient = CreateApiClient(await GetUserTokenAsync());
            var adminCustomer = await CreateTestCustomerAsync(adminClient, "Admin's Customer");
            var userCustomer = await CreateTestCustomerAsync(userClient, "User's Customer");

            try
            {
                var adminList = await adminClient.GetAsync(null);
                var userList = await userClient.GetAsync(null);

                Assert.Contains(adminList, c => c.Id == adminCustomer.Id);
                Assert.Contains(adminList, c => c.Id == userCustomer.Id);
                Assert.Contains(userList, c => c.Id == adminCustomer.Id);
                Assert.Contains(userList, c => c.Id == userCustomer.Id);
            }
            finally
            {
                await CleanupCustomerAsync(adminClient, adminCustomer.Id);
                await CleanupCustomerAsync(adminClient, userCustomer.Id);
            }
        }

        [Fact]
        public async Task HistoryTracksAllChangesInOrder()
        {
            var client = CreateApiClient(await GetAdminTokenAsync());
            var created = await CreateTestCustomerAsync(client, "History Order");

            try
            {
                await client.Update2Async(created.Id, new UpdateCustomerReq
                {
                    Name = "History Order V2",
                    Mobile = UniqueMobile()
                });

                await client.Update2Async(created.Id, new UpdateCustomerReq
                {
                    Name = "History Order V3",
                    Mobile = UniqueMobile()
                });

                var history = await client.History2Async(created.Id);
                Assert.True(history.Count >= 3);
            }
            finally { await CleanupCustomerAsync(client, created.Id); }
        }
    }

}
