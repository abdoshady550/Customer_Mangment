using Customer_Mangment;
using Customer_Mangment_Integrate.Test.Common;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Customer_Mangment_Integrate.Test
{
    public class EndToEndScenarioTests : TestBase
    {
        public EndToEndScenarioTests(WebApplicationFactory<IAssmblyMarker> factory) : base(factory) { }

        [Fact]
        public async Task FullCustomerLifecycle_CreateUpdateDeleteWithAddresses()
        {
            var token = await GetAdminTokenAsync();
            var client = CreateApiClient(token);

            // 1. Create Customer
            var created = await client.Add2Async(new CreateCustomerReq
            {
                Name = "Lifecycle User",
                Mobile = "01000000099",
                Adresses = new List<CreateAddressReq>
                {
                    new CreateAddressReq { Type = 1, Value = "Initial Address" }
                }
            });
            Assert.NotEqual(Guid.Empty, created.Id);

            // 2. Read Customer
            var fetched = (await client.GetAsync(created.Id)).First();
            Assert.Equal("Lifecycle User", fetched.Name);
            Assert.Single(fetched.Addresses);

            // 3. Update Customer
            await client.Update2Async(new UpdateCustomerReq
            {
                CustomerId = created.Id,
                Name = "Lifecycle User Updated",
                Mobile = "01000000098"
            });
            var afterUpdate = (await client.GetAsync(created.Id)).First();
            Assert.Equal("Lifecycle User Updated", afterUpdate.Name);

            // 4. Add Address
            var newAddress = await client.AddAsync(created.Id, new AddAddressReq
            {
                Type = 2,
                Value = "Second Address"
            });
            Assert.NotEqual(Guid.Empty, newAddress.Id);

            // 5. Verify 2 addresses now
            var withTwoAddresses = (await client.GetAsync(created.Id)).First();
            Assert.Equal(2, withTwoAddresses.Addresses.Count);

            // 6. Update Address
            await client.UpdateAsync(new UpdateAddressReq
            {
                AddressId = newAddress.Id,
                Type = 3,
                Value = "Updated Second Address"
            });

            // 7. Delete Address
            await client.DeleteAsync(newAddress.Id);
            var afterAddressDelete = (await client.GetAsync(created.Id)).First();
            Assert.Single(afterAddressDelete.Addresses);

            // 8. Check History
            var history = await client.HistoryAsync(created.Id);
            Assert.NotNull(history.CustomerHistoryDtos);
            Assert.True(history.CustomerHistoryDtos.Count >= 2);

            // 9. Delete Customer
            await client.Delete2Async(created.Id);

            // 10. Verify Deleted
            var ex = await Assert.ThrowsAsync<ApiException<ProblemDetails>>(
                () => client.GetAsync(created.Id));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task AdminVsUser_Permissions_WorkCorrectly()
        {
            var adminToken = await GetAdminTokenAsync();
            var adminClient = CreateApiClient(adminToken);

            var userToken = await GetUserTokenAsync();
            var userClient = CreateApiClient(userToken);

            // ✅ Admin can create
            var adminCustomer = await CreateTestCustomerAsync(adminClient, "Admin Created");
            Assert.NotEqual(Guid.Empty, adminCustomer.Id);

            // ✅ User can create
            var userCustomer = await CreateTestCustomerAsync(userClient, "User Created");
            Assert.NotEqual(Guid.Empty, userCustomer.Id);

            // ✅ User can read
            var customers = await userClient.GetAsync(adminCustomer.Id);
            Assert.NotNull(customers);
            Assert.NotEmpty(customers);

            // ✅ Admin can read
            var adminRead = await adminClient.GetAsync(userCustomer.Id);
            Assert.NotNull(adminRead);

            // ❌ User cannot update — expects 401 or 403
            var updateEx = await Assert.ThrowsAnyAsync<ApiException>(
                () => userClient.Update2Async(new UpdateCustomerReq
                {
                    CustomerId = adminCustomer.Id,
                    Name = "Hacked Name",
                    Mobile = "01099990000"
                }));
            Assert.True(updateEx.StatusCode == 401 || updateEx.StatusCode == 403,
                $"Expected 401 or 403 but got {updateEx.StatusCode}");

            // ❌ User cannot delete — expects 401 or 403
            var deleteEx = await Assert.ThrowsAnyAsync<ApiException>(
                () => userClient.Delete2Async(adminCustomer.Id));
            Assert.True(deleteEx.StatusCode == 401 || deleteEx.StatusCode == 403,
                $"Expected 401 or 403 but got {deleteEx.StatusCode}");

            // ❌ User cannot add address — expects 401 or 403
            var addAddrEx = await Assert.ThrowsAnyAsync<ApiException>(
                () => userClient.AddAsync(adminCustomer.Id, new AddAddressReq
                {
                    Type = 1,
                    Value = "Unauthorized Address"
                }));
            Assert.True(addAddrEx.StatusCode == 401 || addAddrEx.StatusCode == 403,
                $"Expected 401 or 403 but got {addAddrEx.StatusCode}");

            // ❌ User cannot update address — expects 401 or 403
            var firstAddress = adminCustomer.Addresses.First();
            var updateAddrEx = await Assert.ThrowsAnyAsync<ApiException>(
                () => userClient.UpdateAsync(new UpdateAddressReq
                {
                    AddressId = firstAddress.Id,
                    Type = 2,
                    Value = "Unauthorized Update"
                }));
            Assert.True(updateAddrEx.StatusCode == 401 || updateAddrEx.StatusCode == 403,
                $"Expected 401 or 403 but got {updateAddrEx.StatusCode}");

            // ❌ User cannot delete address — expects 401 or 403
            var deleteAddrEx = await Assert.ThrowsAnyAsync<ApiException>(
                () => userClient.DeleteAsync(firstAddress.Id));
            Assert.True(deleteAddrEx.StatusCode == 401 || deleteAddrEx.StatusCode == 403,
                $"Expected 401 or 403 but got {deleteAddrEx.StatusCode}");

            // ✅ Cleanup — only admin can delete
            await adminClient.Delete2Async(adminCustomer.Id);
            await adminClient.Delete2Async(userCustomer.Id);
        }

        // ---- Separate focused permission tests ----

        [Fact]
        public async Task User_CanCreate_Customer()
        {
            var userToken = await GetUserTokenAsync();
            var userClient = CreateApiClient(userToken);

            var customer = await CreateTestCustomerAsync(userClient, "User Created Customer");

            Assert.NotEqual(Guid.Empty, customer.Id);

            // Cleanup
            var adminClient = CreateApiClient(await GetAdminTokenAsync());
            await adminClient.Delete2Async(customer.Id);
        }

        [Fact]
        public async Task User_CanRead_Customers()
        {
            var adminToken = await GetAdminTokenAsync();
            var adminClient = CreateApiClient(adminToken);
            var customer = await CreateTestCustomerAsync(adminClient, "Read Test");

            var userToken = await GetUserTokenAsync();
            var userClient = CreateApiClient(userToken);

            var result = await userClient.GetAsync(customer.Id);

            Assert.NotNull(result);
            Assert.NotEmpty(result);

            // Cleanup
            await adminClient.Delete2Async(customer.Id);
        }

        [Fact]
        public async Task User_CannotUpdate_Customer()
        {
            var adminToken = await GetAdminTokenAsync();
            var adminClient = CreateApiClient(adminToken);
            var customer = await CreateTestCustomerAsync(adminClient, "Update Restriction Test");

            var userToken = await GetUserTokenAsync();
            var userClient = CreateApiClient(userToken);

            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => userClient.Update2Async(new UpdateCustomerReq
                {
                    CustomerId = customer.Id,
                    Name = "Should Not Update",
                    Mobile = "01088880000"
                }));

            Assert.True(ex.StatusCode == 401 || ex.StatusCode == 403,
                $"Expected 401 or 403 but got {ex.StatusCode}");

            // Delete
            await adminClient.Delete2Async(customer.Id);
        }

        [Fact]
        public async Task User_CannotDelete_Customer()
        {
            var adminToken = await GetAdminTokenAsync();
            var adminClient = CreateApiClient(adminToken);
            var customer = await CreateTestCustomerAsync(adminClient, "Delete Restriction Test");

            var userToken = await GetUserTokenAsync();
            var userClient = CreateApiClient(userToken);

            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => userClient.Delete2Async(customer.Id));

            Assert.True(ex.StatusCode == 401 || ex.StatusCode == 403,
                $"Expected 401 or 403 but got {ex.StatusCode}");

            // Delete
            await adminClient.Delete2Async(customer.Id);
        }

        [Fact]
        public async Task HistoryTracking_ReflectsAllChanges()
        {
            var token = await GetAdminTokenAsync();
            var client = CreateApiClient(token);

            // Create
            var customer = await CreateTestCustomerAsync(client, "History Tracker");

            // Update
            await client.Update2Async(new UpdateCustomerReq
            {
                CustomerId = customer.Id,
                Name = "History Tracker v2"
            });

            // Add Address
            var addr = await client.AddAsync(customer.Id, new AddAddressReq
            {
                Type = 1,
                Value = "Track This Address"
            });

            // Update Address
            await client.UpdateAsync(new UpdateAddressReq
            {
                AddressId = addr.Id,
                Type = 2,
                Value = "Track This Address v2"
            });

            // Check History
            var history = await client.HistoryAsync(customer.Id);

            Assert.NotNull(history.CustomerHistoryDtos);
            Assert.True(history.CustomerHistoryDtos.Count >= 1,
                "Customer history should have at least  update");

            Assert.NotNull(history.AddressHistoryDtos);
            Assert.True(history.AddressHistoryDtos.Count >= 1,
                "Address history should have at least one entry");

            // Delete
            await client.Delete2Async(customer.Id);
        }
    }

}
