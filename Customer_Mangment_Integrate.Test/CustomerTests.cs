using Customer_Mangment_Integrate.Test.Common;

namespace Customer_Mangment_Integrate.Test
{
    public class CustomerTests : TestBase
    {
        public CustomerTests(CustomWebApplicationFactory factory) : base(factory) { }

        // ── Create ───────────────────────────────────────────────────────

        [Fact]
        public async Task CreateCustomer_Admin_ReturnsCustomerDto()
        {
            var client = CreateApiClient(await GetAdminTokenAsync());
            var result = await CreateTestCustomerAsync(client, "Ahmed Ali");
            try
            {
                Assert.NotEqual(Guid.Empty, result.Id);
                Assert.Equal("Ahmed Ali", result.Name);
                Assert.Equal(AdminEmail, result.CreatedBy);
                Assert.NotNull(result.Addresses);
            }
            finally { await CleanupCustomerAsync(client, result.Id); }
        }

        [Fact]
        public async Task CreateCustomer_Admin_CreatedByIsAdminEmail()
        {
            var client = CreateApiClient(await GetAdminTokenAsync());
            var result = await CreateTestCustomerAsync(client, "CreatedBy Test");
            try { Assert.Equal(AdminEmail, result.CreatedBy); }
            finally { await CleanupCustomerAsync(client, result.Id); }
        }

        [Fact]
        public async Task CreateCustomer_Admin_UpdatedByIsAdminEmail()
        {
            var client = CreateApiClient(await GetAdminTokenAsync());
            var result = await CreateTestCustomerAsync(client);
            try { Assert.Equal(AdminEmail, result.UpdatedBy); }
            finally { await CleanupCustomerAsync(client, result.Id); }
        }

        [Fact]
        public async Task CreateCustomer_User_ReturnsCustomerDto()
        {
            var adminClient = CreateApiClient(await GetAdminTokenAsync());
            var userClient  = CreateApiClient(await GetUserTokenAsync());
            var result = await CreateTestCustomerAsync(userClient, "User Created");
            try
            {
                Assert.NotEqual(Guid.Empty, result.Id);
                Assert.Equal(UserEmail, result.CreatedBy);
            }
            finally { await CleanupCustomerAsync(adminClient, result.Id); }
        }

        [Fact]
        public async Task CreateCustomer_User_CreatedByIsUserEmail()
        {
            var adminClient = CreateApiClient(await GetAdminTokenAsync());
            var userClient  = CreateApiClient(await GetUserTokenAsync());
            var result = await CreateTestCustomerAsync(userClient, "User CreatedBy");
            try { Assert.Equal(UserEmail, result.CreatedBy); }
            finally { await CleanupCustomerAsync(adminClient, result.Id); }
        }

        [Fact]
        public async Task CreateCustomer_WithOneAddress_ReturnsAddressInList()
        {
            var client = CreateApiClient(await GetAdminTokenAsync());
            var addresses = new List<CreateAddressReq>
            {
                new() { Type = 1, Value = "Address One" }
            };
            var result     = await CreateTestCustomerAsync(client, null, null, addresses);
            var resultAddr = await client.GetAsync(result.Id, null);
            try
            {
                Assert.NotNull(resultAddr);
                Assert.Single(resultAddr);
            }
            finally { await CleanupCustomerAsync(client, result.Id); }
        }

        [Fact]
        public async Task CreateCustomer_WithMultipleAddresses_ReturnsAll()
        {
            var client = CreateApiClient(await GetAdminTokenAsync());
            var result = await client.Add2Async(new CreateCustomerReq
            {
                Name     = "Multi Address",
                Mobile   = UniqueMobile(),
                Adresses = new List<CreateAddressReq>
                {
                    new CreateAddressReq { Type = 1, Value = "Address One" },
                    new CreateAddressReq { Type = 2, Value = "Address Two" }
                }
            });
            var resultAddr = await client.GetAsync(result.Id, null);
            try { Assert.Equal(2, resultAddr.Count); }
            finally { await CleanupCustomerAsync(client, result.Id); }
        }

        [Fact]
        public async Task CreateCustomer_WithNoAddresses_ReturnsEmptyList()
        {
            var client = CreateApiClient(await GetAdminTokenAsync());
            var result = await client.Add2Async(new CreateCustomerReq
            {
                Name     = "No Address Customer",
                Mobile   = UniqueMobile(),
                Adresses = new List<CreateAddressReq>()
            });
            try
            {
                Assert.NotNull(result.Addresses);
                Assert.Empty(result.Addresses);
            }
            finally { await CleanupCustomerAsync(client, result.Id); }
        }

        [Fact]
        public async Task CreateCustomer_WithoutToken_ThrowsUnauthorized()
        {
            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => CreateTestCustomerAsync(CreateApiClient()));
            Assert.Equal(401, ex.StatusCode);
        }

        [Fact]
        public async Task CreateCustomer_DuplicateMobile_ThrowsConflict()
        {
            var client = CreateApiClient(await GetAdminTokenAsync());
            var mobile = UniqueMobile();
            var first  = await CreateTestCustomerAsync(client, "First", mobile);
            try
            {
                var ex = await Assert.ThrowsAnyAsync<ApiException>(
                    () => CreateTestCustomerAsync(client, "Second", mobile));
                Assert.Equal(409, ex.StatusCode);
            }
            finally { await CleanupCustomerAsync(client, first.Id); }
        }

        [Fact]
        public async Task CreateCustomer_ReturnsNonEmptyId()
        {
            var client = CreateApiClient(await GetAdminTokenAsync());
            var result = await CreateTestCustomerAsync(client);
            try { Assert.NotEqual(Guid.Empty, result.Id); }
            finally { await CleanupCustomerAsync(client, result.Id); }
        }

        [Fact]
        public async Task CreateCustomer_AddressHasCorrectCustomerId()
        {
            var client = CreateApiClient(await GetAdminTokenAsync());
            var result = await CreateTestCustomerAsync(client);
            try
            {
                foreach (var address in result.Addresses)
                    Assert.Equal(result.Id, address.CustomerId);
            }
            finally { await CleanupCustomerAsync(client, result.Id); }
        }

        // ── Read ─────────────────────────────────────────────────────────

        [Fact]
        public async Task GetCustomers_NoFilter_ReturnsAllCustomers()
        {
            var client  = CreateApiClient(await GetAdminTokenAsync());
            var created = await CreateTestCustomerAsync(client);
            try
            {
                var list = await client.Get2Async(null);
                Assert.NotNull(list);
                Assert.NotEmpty(list);
            }
            finally { await CleanupCustomerAsync(client, created.Id); }
        }

        [Fact]
        public async Task GetCustomers_WithId_ReturnsSingleCustomer()
        {
            var client  = CreateApiClient(await GetAdminTokenAsync());
            var created = await CreateTestCustomerAsync(client, "Specific");
            try
            {
                var list = await client.Get2Async(created.Id);
                Assert.Single(list);
                Assert.Equal(created.Id, list.First().Id);
                Assert.Equal("Specific", list.First().Name);
            }
            finally { await CleanupCustomerAsync(client, created.Id); }
        }

        [Fact]
        public async Task GetCustomers_WithId_IncludesAddresses()
        {
            var client  = CreateApiClient(await GetAdminTokenAsync());
            var created = await CreateTestCustomerAsync(client);
            try
            {
                var list = await client.Get2Async(created.Id);
                Assert.NotNull(list.First().Addresses);
            }
            finally { await CleanupCustomerAsync(client, created.Id); }
        }

        [Fact]
        public async Task GetCustomers_NonExistentId_ThrowsNotFound()
        {
            var client = CreateApiClient(await GetAdminTokenAsync());
            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => client.Get2Async(Guid.NewGuid()));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task GetCustomers_WithoutToken_ThrowsUnauthorized()
        {
            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => CreateApiClient().Get2Async(null));
            Assert.Equal(401, ex.StatusCode);
        }

        [Fact]
        public async Task GetCustomers_UserToken_ReturnsData()
        {
            var admin   = CreateApiClient(await GetAdminTokenAsync());
            var created = await CreateTestCustomerAsync(admin);
            try
            {
                var user = CreateApiClient(await GetUserTokenAsync());
                var list = await user.Get2Async(null);
                Assert.NotNull(list);
                Assert.NotEmpty(list);
            }
            finally { await CleanupCustomerAsync(admin, created.Id); }
        }

        [Fact]
        public async Task GetCustomers_AfterCreate_ContainsCreatedCustomer()
        {
            var client  = CreateApiClient(await GetAdminTokenAsync());
            var created = await CreateTestCustomerAsync(client, "Find Me");
            try
            {
                var list = await client.Get2Async(null);
                Assert.Contains(list, c => c.Id == created.Id);
            }
            finally { await CleanupCustomerAsync(client, created.Id); }
        }

        // ── Update ───────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateCustomer_Admin_UpdatesName()
        {
            var client  = CreateApiClient(await GetAdminTokenAsync());
            var created = await CreateTestCustomerAsync(client, "Old Name");
            try
            {
                await client.Update2Async(created.Id, new UpdateCustomerReq
                {
                    Name   = "New Name",
                    Mobile = UniqueMobile()
                });
                var updated = (await client.Get2Async(created.Id)).First();
                Assert.Equal("New Name", updated.Name);
            }
            finally { await CleanupCustomerAsync(client, created.Id); }
        }

        [Fact]
        public async Task UpdateCustomer_Admin_UpdatedByIsAdminEmail()
        {
            var client  = CreateApiClient(await GetAdminTokenAsync());
            var created = await CreateTestCustomerAsync(client, "UpdatedBy Check");
            try
            {
                await client.Update2Async(created.Id, new UpdateCustomerReq
                {
                    Name   = "Updated",
                    Mobile = UniqueMobile()
                });
                var updated = (await client.Get2Async(created.Id)).First();
                Assert.Equal(AdminEmail, updated.UpdatedBy);
            }
            finally { await CleanupCustomerAsync(client, created.Id); }
        }

        [Fact]
        public async Task UpdateCustomer_Admin_UpdatesMobile()
        {
            var client    = CreateApiClient(await GetAdminTokenAsync());
            var created   = await CreateTestCustomerAsync(client);
            var newMobile = UniqueMobile();
            try
            {
                await client.Update2Async(created.Id, new UpdateCustomerReq
                {
                    Name   = created.Name,
                    Mobile = newMobile
                });
                var updated = (await client.Get2Async(created.Id)).First();
                Assert.Equal(newMobile, updated.Mobile);
            }
            finally { await CleanupCustomerAsync(client, created.Id); }
        }

        [Fact]
        public async Task UpdateCustomer_NonExistentId_ThrowsNotFound()
        {
            var client = CreateApiClient(await GetAdminTokenAsync());
            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => client.Update2Async(Guid.NewGuid(), new UpdateCustomerReq
                {
                    Name   = "Ghost",
                    Mobile = "01000000000"
                }));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task UpdateCustomer_WithoutToken_ThrowsUnauthorized()
        {
            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => CreateApiClient().Update2Async(Guid.NewGuid(), new UpdateCustomerReq
                {
                    Name   = "Test",
                    Mobile = "01000000000"
                }));
            Assert.Equal(401, ex.StatusCode);
        }

        [Fact]
        public async Task UpdateCustomer_User_ThrowsForbiddenOrUnauthorized()
        {
            var admin   = CreateApiClient(await GetAdminTokenAsync());
            var created = await CreateTestCustomerAsync(admin);
            try
            {
                var user = CreateApiClient(await GetUserTokenAsync());
                var ex = await Assert.ThrowsAnyAsync<ApiException>(
                    () => user.Update2Async(created.Id, new UpdateCustomerReq
                    {
                        Name   = "Should Fail",
                        Mobile = UniqueMobile()
                    }));
                Assert.True(ex.StatusCode == 401 || ex.StatusCode == 403);
            }
            finally { await CleanupCustomerAsync(admin, created.Id); }
        }

        // ── Delete ───────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteCustomer_Admin_DeletesSuccessfully()
        {
            var client  = CreateApiClient(await GetAdminTokenAsync());
            var created = await CreateTestCustomerAsync(client, "To Delete");

            await client.Delete2Async(created.Id);

            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => client.Get2Async(created.Id));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task DeleteCustomer_Admin_CustomerNoLongerInList()
        {
            var client  = CreateApiClient(await GetAdminTokenAsync());
            var created = await CreateTestCustomerAsync(client, "Remove Me");

            await client.Delete2Async(created.Id);

            var list = await client.Get2Async(null);
            Assert.DoesNotContain(list, c => c.Id == created.Id);
        }

        [Fact]
        public async Task DeleteCustomer_NonExistentId_ThrowsNotFound()
        {
            var client = CreateApiClient(await GetAdminTokenAsync());
            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => client.Delete2Async(Guid.NewGuid()));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task DeleteCustomer_WithoutToken_ThrowsUnauthorized()
        {
            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => CreateApiClient().Delete2Async(Guid.NewGuid()));
            Assert.Equal(401, ex.StatusCode);
        }

        [Fact]
        public async Task DeleteCustomer_User_ThrowsForbiddenOrUnauthorized()
        {
            var admin   = CreateApiClient(await GetAdminTokenAsync());
            var created = await CreateTestCustomerAsync(admin);
            try
            {
                var user = CreateApiClient(await GetUserTokenAsync());
                var ex = await Assert.ThrowsAnyAsync<ApiException>(
                    () => user.Delete2Async(created.Id));
                Assert.True(ex.StatusCode == 401 || ex.StatusCode == 403);
            }
            finally { await CleanupCustomerAsync(admin, created.Id); }
        }

        [Fact]
        public async Task DeleteCustomer_Twice_SecondCallThrowsNotFound()
        {
            var client  = CreateApiClient(await GetAdminTokenAsync());
            var created = await CreateTestCustomerAsync(client, "Delete Twice");

            await client.Delete2Async(created.Id);

            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => client.Delete2Async(created.Id));
            Assert.Equal(404, ex.StatusCode);
        }

        // ── History ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetHistory_AfterUpdate_ContainsMultipleEntries()
        {
            var client  = CreateApiClient(await GetAdminTokenAsync());
            var created = await CreateTestCustomerAsync(client, "History Update");
            try
            {
                await client.Update2Async(created.Id, new UpdateCustomerReq
                {
                    Name   = "History Updated",
                    Mobile = UniqueMobile()
                });
                var history = await client.History2Async(created.Id);
                Assert.True(history.Count >= 2, "Should have at least 2 entries: create + update");
            }
            finally { await CleanupCustomerAsync(client, created.Id); }
        }

        [Fact]
        public async Task GetHistory_AfterDelete_HistoryStillAccessible()
        {
            var client  = CreateApiClient(await GetAdminTokenAsync());
            var created = await CreateTestCustomerAsync(client, "Deleted But History");

            await client.Delete2Async(created.Id);

            var history = await client.History2Async(created.Id);
            Assert.NotNull(history);
            Assert.NotEmpty(history);
        }

        [Fact]
        public async Task GetHistory_ReturnsCustomerHistoryDtos()
        {
            var client  = CreateApiClient(await GetAdminTokenAsync());
            var created = await CreateTestCustomerAsync(client);
            try
            {
                var history = await client.History2Async(created.Id);
                Assert.NotNull(history);
            }
            finally { await CleanupCustomerAsync(client, created.Id); }
        }

        [Fact]
        public async Task GetHistory_NonExistentId_ThrowsNotFound()
        {
            var client = CreateApiClient(await GetAdminTokenAsync());
            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => client.History2Async(Guid.NewGuid()));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task GetHistory_WithoutToken_ThrowsUnauthorized()
        {
            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => CreateApiClient().History2Async(Guid.NewGuid()));
            Assert.Equal(401, ex.StatusCode);
        }

        [Fact]
        public async Task GetHistory_UserToken_CanAccessHistory()
        {
            var admin   = CreateApiClient(await GetAdminTokenAsync());
            var created = await CreateTestCustomerAsync(admin);
            try
            {
                var user    = CreateApiClient(await GetUserTokenAsync());
                var history = await user.History2Async(created.Id);
                Assert.NotNull(history);
            }
            finally { await CleanupCustomerAsync(admin, created.Id); }
        }
    }
}
