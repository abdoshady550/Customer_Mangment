using Customer_Mangment;
using Customer_Mangment_Integrate.Test.Common;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Customer_Mangment_Integrate.Test
{
    public class AdminPermissionTests : TestBase
    {
        public AdminPermissionTests(WebApplicationFactory<IAssmblyMarker> factory) : base(factory) { }

        [Fact]
        public async Task Admin_CanCreate_Customer()
        {
            var client = CreateApiClient(await GetAdminTokenAsync());
            var result = await CreateTestCustomerAsync(client, "Admin Create");
            try
            {
                Assert.NotEqual(Guid.Empty, result.Id);
                Assert.Equal(AdminEmail, result.CreatedBy);
            }
            finally { await CleanupCustomerAsync(client, result.Id); }
        }

        [Fact]
        public async Task Admin_CanRead_AllCustomers()
        {
            var client = CreateApiClient(await GetAdminTokenAsync());
            var created = await CreateTestCustomerAsync(client);
            try
            {
                var list = await client.GetAsync(null);
                Assert.NotNull(list);
            }
            finally { await CleanupCustomerAsync(client, created.Id); }
        }

        [Fact]
        public async Task Admin_CanRead_SpecificCustomer()
        {
            var client = CreateApiClient(await GetAdminTokenAsync());
            var created = await CreateTestCustomerAsync(client);
            try
            {
                var list = await client.GetAsync(created.Id);
                Assert.Single(list);
                Assert.Equal(created.Id, list.First().Id);
            }
            finally { await CleanupCustomerAsync(client, created.Id); }
        }

        [Fact]
        public async Task Admin_CanUpdate_Customer_And_UpdatedBy_IsAdmin()
        {
            var client = CreateApiClient(await GetAdminTokenAsync());
            var created = await CreateTestCustomerAsync(client, "Admin Update");
            try
            {
                await client.Update2Async(new UpdateCustomerReq
                {
                    CustomerId = created.Id,
                    Name = "Admin Updated",
                    Mobile = UniqueMobile()
                });

                var updated = (await client.GetAsync(created.Id)).First();
                Assert.Equal("Admin Updated", updated.Name);
                Assert.Equal(AdminEmail, updated.UpdatedBy);
            }
            finally { await CleanupCustomerAsync(client, created.Id); }
        }

        [Fact]
        public async Task Admin_CanDelete_Customer()
        {
            var client = CreateApiClient(await GetAdminTokenAsync());
            var created = await CreateTestCustomerAsync(client, "Admin Delete");

            await client.Delete2Async(created.Id);

            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => client.GetAsync(created.Id));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task Admin_CanAddAddress()
        {
            var client = CreateApiClient(await GetAdminTokenAsync());
            var customer = await CreateTestCustomerAsync(client);
            try
            {
                var address = await client.AddAsync(customer.Id, new AddAddressReq
                {
                    Type = 1,
                    Value = "Admin Address"
                });

                Assert.NotEqual(Guid.Empty, address.Id);
            }
            finally { await CleanupCustomerAsync(client, customer.Id); }
        }

        [Fact]
        public async Task Admin_CanUpdateAddress()
        {
            var client = CreateApiClient(await GetAdminTokenAsync());
            var customer = await CreateTestCustomerAsync(client);
            try
            {
                var address = await AddAddressAsync(client, customer.Id);

                await client.UpdateAsync(new UpdateAddressReq
                {
                    AddressId = address.Id,
                    Type = 1,
                    Value = "Admin Updated Address"
                });

                var updated = (await client.GetAsync(customer.Id)).First();
                Assert.Contains(updated.Addresses,
                    a => a.Id == address.Id && a.Value == "Admin Updated Address");
            }
            finally { await CleanupCustomerAsync(client, customer.Id); }
        }

        [Fact]
        public async Task Admin_CanDeleteAddress()
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
        public async Task Admin_CanRead_History()
        {
            var client = CreateApiClient(await GetAdminTokenAsync());
            var customer = await CreateTestCustomerAsync(client);
            try
            {
                var history = await client.History2Async(customer.Id);
                Assert.NotNull(history);
                Assert.NotEmpty(history);
            }
            finally { await CleanupCustomerAsync(client, customer.Id); }
        }

        [Fact]
        public async Task Admin_CanRead_History_AfterDelete()
        {
            var client = CreateApiClient(await GetAdminTokenAsync());
            var customer = await CreateTestCustomerAsync(client);

            await client.Delete2Async(customer.Id);

            var history = await client.History2Async(customer.Id);
            Assert.NotNull(history);
        }

        [Fact]
        public async Task Admin_CanGenerateToken()
        {
            var result = await CreateApiClient().GenerateTokenAsync(new GenerateTokenQuery
            {
                Email = AdminEmail,
                Password = AdminPassword
            });

            Assert.NotNull(result.AccessToken);
        }

        [Fact]
        public async Task Admin_CanRefreshToken()
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

            Assert.NotNull(refreshed.AccessToken);
        }
    }


    //  User

    public class UserPermissionTests : TestBase
    {
        public UserPermissionTests(WebApplicationFactory<IAssmblyMarker> factory) : base(factory) { }

        //  Allowed 

        [Fact]
        public async Task User_CanCreate_Customer_And_CreatedBy_IsUser()
        {
            var adminClient = CreateApiClient(await GetAdminTokenAsync());
            var userClient = CreateApiClient(await GetUserTokenAsync());
            var result = await CreateTestCustomerAsync(userClient, "User Create");
            try
            {
                Assert.NotEqual(Guid.Empty, result.Id);
                Assert.Equal(UserEmail, result.CreatedBy);
            }
            finally { await CleanupCustomerAsync(adminClient, result.Id); }
        }

        [Fact]
        public async Task User_CanRead_AllCustomers()
        {
            var admin = CreateApiClient(await GetAdminTokenAsync());
            var created = await CreateTestCustomerAsync(admin);
            try
            {
                var user = CreateApiClient(await GetUserTokenAsync());
                var list = await user.GetAsync(null);
                Assert.NotNull(list);
                Assert.NotEmpty(list);
            }
            finally { await CleanupCustomerAsync(admin, created.Id); }
        }

        [Fact]
        public async Task User_CanRead_SpecificCustomer()
        {
            var admin = CreateApiClient(await GetAdminTokenAsync());
            var created = await CreateTestCustomerAsync(admin);
            try
            {
                var user = CreateApiClient(await GetUserTokenAsync());
                var result = await user.GetAsync(created.Id);
                Assert.Single(result);
                Assert.Equal(created.Id, result.First().Id);
            }
            finally { await CleanupCustomerAsync(admin, created.Id); }
        }

        [Fact]
        public async Task User_CanRead_CustomerTheyCreated()
        {
            var adminClient = CreateApiClient(await GetAdminTokenAsync());
            var userClient = CreateApiClient(await GetUserTokenAsync());
            var created = await CreateTestCustomerAsync(userClient, "User's Own");
            try
            {
                var fetched = (await userClient.GetAsync(created.Id)).First();
                Assert.Equal(created.Id, fetched.Id);
            }
            finally { await CleanupCustomerAsync(adminClient, created.Id); }
        }

        [Fact]
        public async Task User_CanGenerateToken()
        {
            var result = await CreateApiClient().GenerateTokenAsync(new GenerateTokenQuery
            {
                Email = UserEmail,
                Password = UserPassword
            });

            Assert.NotNull(result.AccessToken);
        }

        [Fact]
        public async Task User_CanRefreshToken()
        {
            var client = CreateApiClient();
            var initial = await client.GenerateTokenAsync(new GenerateTokenQuery
            {
                Email = UserEmail,
                Password = UserPassword
            });

            var refreshed = await client.RefreshTokenAsync(new RefreshTokenQuery
            {
                RefreshToken = initial.RefreshToken,
                ExpiredAccessToken = initial.AccessToken
            });

            Assert.NotNull(refreshed.AccessToken);
        }

        [Fact]
        public async Task User_CanRead_History()
        {
            var admin = CreateApiClient(await GetAdminTokenAsync());
            var created = await CreateTestCustomerAsync(admin);
            try
            {
                var user = CreateApiClient(await GetUserTokenAsync());
                var history = await user.History2Async(created.Id);
                Assert.NotNull(history);
            }
            finally { await CleanupCustomerAsync(admin, created.Id); }
        }

        // Forbidden

        [Fact]
        public async Task User_CannotUpdate_Customer()
        {
            var admin = CreateApiClient(await GetAdminTokenAsync());
            var created = await CreateTestCustomerAsync(admin);
            try
            {
                var user = CreateApiClient(await GetUserTokenAsync());

                var ex = await Assert.ThrowsAnyAsync<ApiException>(
                    () => user.Update2Async(new UpdateCustomerReq
                    {
                        CustomerId = created.Id,
                        Name = "Should Not Update",
                        Mobile = UniqueMobile()
                    }));

                Assert.True(ex.StatusCode == 401 || ex.StatusCode == 403,
                    $"Expected 401/403 but got {ex.StatusCode}");
            }
            finally { await CleanupCustomerAsync(admin, created.Id); }
        }

        [Fact]
        public async Task User_CannotDelete_Customer()
        {
            var admin = CreateApiClient(await GetAdminTokenAsync());
            var created = await CreateTestCustomerAsync(admin);
            try
            {
                var user = CreateApiClient(await GetUserTokenAsync());

                var ex = await Assert.ThrowsAnyAsync<ApiException>(
                    () => user.Delete2Async(created.Id));

                Assert.True(ex.StatusCode == 401 || ex.StatusCode == 403,
                    $"Expected 401/403 but got {ex.StatusCode}");
            }
            finally { await CleanupCustomerAsync(admin, created.Id); }
        }

        [Fact]
        public async Task User_CannotAddAddress()
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

                Assert.True(ex.StatusCode == 401 || ex.StatusCode == 403,
                    $"Expected 401/403 but got {ex.StatusCode}");
            }
            finally { await CleanupCustomerAsync(admin, customer.Id); }
        }

        [Fact]
        public async Task User_CannotUpdateAddress()
        {
            var admin = CreateApiClient(await GetAdminTokenAsync());
            var customer = await CreateTestCustomerAsync(admin);
            try
            {
                var address = await AddAddressAsync(admin, customer.Id);
                var user = CreateApiClient(await GetUserTokenAsync());

                var ex = await Assert.ThrowsAnyAsync<ApiException>(
                    () => user.UpdateAsync(new UpdateAddressReq
                    {
                        AddressId = address.Id,
                        Type = 1,
                        Value = "Unauthorized"
                    }));

                Assert.True(ex.StatusCode == 401 || ex.StatusCode == 403,
                    $"Expected 401/403 but got {ex.StatusCode}");
            }
            finally { await CleanupCustomerAsync(admin, customer.Id); }
        }

        [Fact]
        public async Task User_CannotDeleteAddress()
        {
            var admin = CreateApiClient(await GetAdminTokenAsync());
            var customer = await CreateTestCustomerAsync(admin);
            try
            {
                var address = await AddAddressAsync(admin, customer.Id);
                var user = CreateApiClient(await GetUserTokenAsync());

                var ex = await Assert.ThrowsAnyAsync<ApiException>(
                    () => user.DeleteAsync(address.Id));

                Assert.True(ex.StatusCode == 401 || ex.StatusCode == 403,
                    $"Expected 401/403 but got {ex.StatusCode}");
            }
            finally { await CleanupCustomerAsync(admin, customer.Id); }
        }

        [Fact]
        public async Task User_CannotUpdate_CustomerCreatedByThemselves()
        {
            var adminClient = CreateApiClient(await GetAdminTokenAsync());
            var userClient = CreateApiClient(await GetUserTokenAsync());
            var created = await CreateTestCustomerAsync(userClient, "User Own Customer");
            try
            {
                var ex = await Assert.ThrowsAnyAsync<ApiException>(
                    () => userClient.Update2Async(new UpdateCustomerReq
                    {
                        CustomerId = created.Id,
                        Name = "Should Still Fail",
                        Mobile = UniqueMobile()
                    }));

                Assert.True(ex.StatusCode == 401 || ex.StatusCode == 403);
            }
            finally { await CleanupCustomerAsync(adminClient, created.Id); }
        }

        [Fact]
        public async Task User_CannotDelete_CustomerCreatedByThemselves()
        {
            var adminClient = CreateApiClient(await GetAdminTokenAsync());
            var userClient = CreateApiClient(await GetUserTokenAsync());
            var created = await CreateTestCustomerAsync(userClient, "User Own Delete");
            try
            {
                var ex = await Assert.ThrowsAnyAsync<ApiException>(
                    () => userClient.Delete2Async(created.Id));

                Assert.True(ex.StatusCode == 401 || ex.StatusCode == 403);
            }
            finally { await CleanupCustomerAsync(adminClient, created.Id); }
        }
    }


    // Anonymous

    public class AnonymousPermissionTests : TestBase
    {
        public AnonymousPermissionTests(WebApplicationFactory<IAssmblyMarker> factory) : base(factory) { }

        [Fact]
        public async Task Anonymous_CannotCreate_Customer()
        {
            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => CreateTestCustomerAsync(CreateApiClient()));
            Assert.Equal(401, ex.StatusCode);
        }

        [Fact]
        public async Task Anonymous_CannotRead_Customers()
        {
            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => CreateApiClient().GetAsync(null));
            Assert.Equal(401, ex.StatusCode);
        }

        [Fact]
        public async Task Anonymous_CannotRead_SpecificCustomer()
        {
            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => CreateApiClient().GetAsync(Guid.NewGuid()));
            Assert.Equal(401, ex.StatusCode);
        }

        [Fact]
        public async Task Anonymous_CannotUpdate_Customer()
        {
            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => CreateApiClient().Update2Async(new UpdateCustomerReq
                {
                    CustomerId = Guid.NewGuid(),
                    Name = "Test",
                    Mobile = "01000000000"
                }));
            Assert.Equal(401, ex.StatusCode);
        }

        [Fact]
        public async Task Anonymous_CannotDelete_Customer()
        {
            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => CreateApiClient().Delete2Async(Guid.NewGuid()));
            Assert.Equal(401, ex.StatusCode);
        }

        [Fact]
        public async Task Anonymous_CannotAddAddress()
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
        public async Task Anonymous_CannotUpdateAddress()
        {
            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => CreateApiClient().UpdateAsync(new UpdateAddressReq
                {
                    AddressId = Guid.NewGuid(),
                    Type = 1,
                    Value = "Test"
                }));
            Assert.Equal(401, ex.StatusCode);
        }

        [Fact]
        public async Task Anonymous_CannotDeleteAddress()
        {
            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => CreateApiClient().DeleteAsync(Guid.NewGuid()));
            Assert.Equal(401, ex.StatusCode);
        }

        [Fact]
        public async Task Anonymous_CannotRead_History()
        {
            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => CreateApiClient().History2Async(Guid.NewGuid()));
            Assert.Equal(401, ex.StatusCode);
        }

        [Fact]
        public async Task Anonymous_CanGenerateToken_WithValidCredentials()
        {
            var result = await CreateApiClient().GenerateTokenAsync(new GenerateTokenQuery
            {
                Email = AdminEmail,
                Password = AdminPassword
            });
            Assert.NotNull(result.AccessToken);
        }

        [Fact]
        public async Task Anonymous_CanRefreshToken_WithValidTokens()
        {
            var client = CreateApiClient();
            var initial = await client.GenerateTokenAsync(new GenerateTokenQuery
            {
                Email = UserEmail,
                Password = UserPassword
            });

            var refreshed = await client.RefreshTokenAsync(new RefreshTokenQuery
            {
                RefreshToken = initial.RefreshToken,
                ExpiredAccessToken = initial.AccessToken
            });

            Assert.NotNull(refreshed.AccessToken);
        }
    }
}
