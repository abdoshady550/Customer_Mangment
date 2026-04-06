using Customer_Mangment;
using Customer_Mangment_Integrate.Test.Common;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Customer_Mangment_Integrate.Test
{
    public class MultiTenancyTests : TestBase
    {
        public MultiTenancyTests(WebApplicationFactory<IAssmblyMarker> factory) : base(factory) { }

        private const string TenantAlahly = "alahly";
        private const string TenantMeccano = "meccano";


        private Client ApiClientForTenant(string tenantId, string? accessToken = null)
        {
            var http = _factory.CreateClient();
            http.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId);
            if (!string.IsNullOrEmpty(accessToken))
                http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);
            return new Client(http) { BaseUrl = http.BaseAddress?.ToString() ?? "" };
        }

        private async Task<string> GetAdminTokenForTenantAsync(string tenantId)
        {
            var http = _factory.CreateClient();
            http.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId);
            var client = new Client(http) { BaseUrl = http.BaseAddress?.ToString() ?? "" };
            var response = await client.GenerateTokenAsync(new GenerateTokenQuery
            {
                Email = AdminEmail,
                Password = AdminPassword
            });
            return response.AccessToken;
        }

        private async Task<string> GetUserTokenForTenantAsync(string tenantId)
        {
            var http = _factory.CreateClient();
            http.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId);
            var client = new Client(http) { BaseUrl = http.BaseAddress?.ToString() ?? "" };
            var response = await client.GenerateTokenAsync(new GenerateTokenQuery
            {
                Email = UserEmail,
                Password = UserPassword
            });
            return response.AccessToken;
        }

        private async Task CleanupTenantCustomerAsync(string tenantId, Guid customerId)
        {
            try
            {
                var token = await GetAdminTokenForTenantAsync(tenantId);
                var client = ApiClientForTenant(tenantId, token);
                await client.Delete2Async(customerId);
            }
            catch { }
        }

        //  Header check  

        [Fact]
        public async Task Request_WithoutTenantHeader_Returns400()
        {
            var http = _factory.CreateClient();
            var response = await http.GetAsync("api/Customer/get");
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Request_WithEmptyTenantHeader_Returns400()
        {
            var http = _factory.CreateClient();
            http.DefaultRequestHeaders.Add("X-Tenant-Id", "");
            var response = await http.GetAsync("api/Customer/get");
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Request_WithUnknownTenantId_Returns404()
        {
            var http = _factory.CreateClient();
            http.DefaultRequestHeaders.Add("X-Tenant-Id", "totally-unknown-tenant-xyz");
            var response = await http.GetAsync("api/Customer/get");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Request_WithValidTenantId_DoesNotReturn400Or404ForTenant()
        {
            var http = _factory.CreateClient();
            http.DefaultRequestHeaders.Add("X-Tenant-Id", DefaultTenantId);
            var response = await http.GetAsync("api/Customer/get");
            Assert.NotEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotEqual(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        // endpoints bypass tenant check 

        [Fact]
        public async Task GenerateToken_WithoutTenantHeader_Succeeds()
        {
            var result = await CreateApiClient().GenerateTokenAsync(new GenerateTokenQuery
            {
                Email = AdminEmail,
                Password = AdminPassword
            });
            Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
        }

        [Fact]
        public async Task RefreshToken_WithoutTenantHeader_Succeeds()
        {
            var http = _factory.CreateClient();
            var client = new Client(http) { BaseUrl = http.BaseAddress?.ToString() ?? "" };

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

            Assert.False(string.IsNullOrWhiteSpace(refreshed.AccessToken));
        }

        // Tenant claim

        [Fact]
        public async Task Token_IssuedForOneTenant_Rejected_WhenUsedWithDifferentTenant()
        {
            var token = await GetAdminTokenAsync();

            var http = _factory.CreateClient();
            http.DefaultRequestHeaders.Add("X-Tenant-Id", TenantAlahly);
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await http.GetAsync("api/Customer/get");
            Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Token_IssuedForAlahly_Rejected_WhenUsedWithMeccano()
        {
            var token = await GetAdminTokenForTenantAsync(TenantAlahly);

            var http = _factory.CreateClient();
            http.DefaultRequestHeaders.Add("X-Tenant-Id", TenantMeccano);
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await http.GetAsync("api/Customer/get");
            Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Token_IssuedForTenant_Accepted_WhenUsedWithSameTenant()
        {
            var token = await GetAdminTokenAsync();
            var client = CreateApiClient(token);
            var customers = await client.Get2Async(null);
            Assert.NotNull(customers);
        }

        [Fact]
        public async Task UserToken_IssuedForTenant_Rejected_OnDifferentTenant()
        {
            var token = await GetUserTokenAsync();

            var http = _factory.CreateClient();
            http.DefaultRequestHeaders.Add("X-Tenant-Id", TenantAlahly);
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await http.GetAsync("api/Customer/get");
            Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Request_WithValidToken_AndNoTenantHeader_Returns400()
        {
            var token = await GetAdminTokenAsync();

            var http = _factory.CreateClient();
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await http.GetAsync("api/Customer/get");
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Data isolation

        [Fact]
        public async Task CustomerCreatedInDefaultTenant_NotVisibleInAlahly()
        {
            var defaultToken = await GetAdminTokenAsync();
            var alahlyToken = await GetAdminTokenForTenantAsync(TenantAlahly);

            var defaultClient = CreateApiClient(defaultToken);
            var alahlyClient = ApiClientForTenant(TenantAlahly, alahlyToken);

            var created = await defaultClient.Add2Async(new CreateCustomerReq
            {
                Name = "Demo-Only Customer",
                Mobile = UniqueMobile(),
                Adresses = new List<CreateAddressReq>()
            });

            try
            {
                var alahlyList = await alahlyClient.Get2Async(null);
                Assert.DoesNotContain(alahlyList, c => c.Id == created.Id);
            }
            finally { await CleanupCustomerAsync(defaultClient, created.Id); }
        }

        [Fact]
        public async Task CustomerCreatedInAlahly_NotVisibleInDefaultTenant()
        {
            var defaultToken = await GetAdminTokenAsync();
            var alahlyToken = await GetAdminTokenForTenantAsync(TenantAlahly);

            var defaultClient = CreateApiClient(defaultToken);
            var alahlyClient = ApiClientForTenant(TenantAlahly, alahlyToken);

            var created = await alahlyClient.Add2Async(new CreateCustomerReq
            {
                Name = "Alahly-Only Customer",
                Mobile = UniqueMobile(),
                Adresses = new List<CreateAddressReq>()
            });

            try
            {
                var defaultList = await defaultClient.Get2Async(null);
                Assert.DoesNotContain(defaultList, c => c.Id == created.Id);
            }
            finally { await CleanupTenantCustomerAsync(TenantAlahly, created.Id); }
        }

        [Fact]
        public async Task CrossTenantLookupById_Returns404()
        {
            var defaultToken = await GetAdminTokenAsync();
            var alahlyToken = await GetAdminTokenForTenantAsync(TenantAlahly);

            var defaultClient = CreateApiClient(defaultToken);
            var alahlyClient = ApiClientForTenant(TenantAlahly, alahlyToken);

            var created = await defaultClient.Add2Async(new CreateCustomerReq
            {
                Name = "Isolation Target",
                Mobile = UniqueMobile(),
                Adresses = new List<CreateAddressReq>()
            });

            try
            {
                var ex = await Assert.ThrowsAnyAsync<ApiException>(
                    () => alahlyClient.Get2Async(created.Id));
                Assert.Equal(404, ex.StatusCode);
            }
            finally { await CleanupCustomerAsync(defaultClient, created.Id); }
        }

        [Fact]
        public async Task TwoTenantsHaveIndependentCustomerSets()
        {
            var defaultToken = await GetAdminTokenAsync();
            var alahlyToken = await GetAdminTokenForTenantAsync(TenantAlahly);

            var defaultClient = CreateApiClient(defaultToken);
            var alahlyClient = ApiClientForTenant(TenantAlahly, alahlyToken);

            var defaultCustomer = await defaultClient.Add2Async(new CreateCustomerReq
            {
                Name = "Default Customer",
                Mobile = UniqueMobile(),
                Adresses = new List<CreateAddressReq>()
            });
            var alahlyCustomer = await alahlyClient.Add2Async(new CreateCustomerReq
            {
                Name = "Alahly Customer",
                Mobile = UniqueMobile(),
                Adresses = new List<CreateAddressReq>()
            });

            try
            {
                Assert.NotEqual(defaultCustomer.Id, alahlyCustomer.Id);

                var defaultResult = await defaultClient.Get2Async(defaultCustomer.Id);
                var alahlyResult = await alahlyClient.Get2Async(alahlyCustomer.Id);

                Assert.Single(defaultResult);
                Assert.Single(alahlyResult);
            }
            finally
            {
                await CleanupCustomerAsync(defaultClient, defaultCustomer.Id);
                await CleanupTenantCustomerAsync(TenantAlahly, alahlyCustomer.Id);
            }
        }

        // Cross-tenant write operations are blocked          

        [Fact]
        public async Task UpdateCustomer_UsingDifferentTenantToken_Returns403()
        {
            var demoToken = await GetAdminTokenAsync();

            var http = _factory.CreateClient();
            http.DefaultRequestHeaders.Add("X-Tenant-Id", TenantAlahly);
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", demoToken);

            var response = await http.PutAsJsonAsync(
                $"api/Customer/update?CustomerId={Guid.NewGuid()}",
                new { name = "Hacked", mobile = UniqueMobile() });

            Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task DeleteCustomer_UsingDifferentTenantToken_Returns403()
        {
            var demoToken = await GetAdminTokenAsync();

            var http = _factory.CreateClient();
            http.DefaultRequestHeaders.Add("X-Tenant-Id", TenantAlahly);
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", demoToken);

            var response = await http.DeleteAsync(
                $"api/Customer/delete?CustomerId={Guid.NewGuid()}");

            Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
        }

        // Business rules are tenant-scoped

        [Fact]
        public async Task SameMobileInDifferentTenants_BothSucceed()
        {
            var defaultToken = await GetAdminTokenAsync();
            var alahlyToken = await GetAdminTokenForTenantAsync(TenantAlahly);

            var defaultClient = CreateApiClient(defaultToken);
            var alahlyClient = ApiClientForTenant(TenantAlahly, alahlyToken);

            var sharedMobile = UniqueMobile();

            var defaultCustomer = await defaultClient.Add2Async(new CreateCustomerReq
            {
                Name = "Default",
                Mobile = sharedMobile,
                Adresses = new List<CreateAddressReq>()
            });
            var alahlyCustomer = await alahlyClient.Add2Async(new CreateCustomerReq
            {
                Name = "Alahly",
                Mobile = sharedMobile,
                Adresses = new List<CreateAddressReq>()
            });

            try
            {
                Assert.NotEqual(defaultCustomer.Id, alahlyCustomer.Id);
                Assert.Equal(sharedMobile, defaultCustomer.Mobile);
                Assert.Equal(sharedMobile, alahlyCustomer.Mobile);
            }
            finally
            {
                await CleanupCustomerAsync(defaultClient, defaultCustomer.Id);
                await CleanupTenantCustomerAsync(TenantAlahly, alahlyCustomer.Id);
            }
        }

        [Fact]
        public async Task DuplicateMobile_WithinSameTenant_StillConflicts()
        {
            var token = await GetAdminTokenAsync();
            var client = CreateApiClient(token);
            var mobile = UniqueMobile();

            var first = await client.Add2Async(new CreateCustomerReq
            {
                Name = "First",
                Mobile = mobile,
                Adresses = new List<CreateAddressReq>()
            });

            try
            {
                var ex = await Assert.ThrowsAnyAsync<ApiException>(
                    () => client.Add2Async(new CreateCustomerReq
                    {
                        Name = "Second",
                        Mobile = mobile,
                        Adresses = new List<CreateAddressReq>()
                    }));

                Assert.Equal(409, ex.StatusCode);
            }
            finally { await CleanupCustomerAsync(client, first.Id); }
        }

        [Fact]
        public async Task AddressCreatedInOneTenant_NotVisibleInAnother()
        {
            var defaultToken = await GetAdminTokenAsync();
            var alahlyToken = await GetAdminTokenForTenantAsync(TenantAlahly);

            var defaultClient = CreateApiClient(defaultToken);
            var alahlyClient = ApiClientForTenant(TenantAlahly, alahlyToken);

            var customer = await defaultClient.Add2Async(new CreateCustomerReq
            {
                Name = "Addr Isolation",
                Mobile = UniqueMobile(),
                Adresses = new List<CreateAddressReq>()
            });

            try
            {
                var address = await defaultClient.AddAsync(customer.Id, new AddAddressReq
                {
                    Type = 1,
                    Value = "Demo Address"
                });

                var alahlyAddresses = await alahlyClient.GetAsync(null, null);
                Assert.DoesNotContain(alahlyAddresses, a => a.Id == address.Id);
            }
            finally { await CleanupCustomerAsync(defaultClient, customer.Id); }
        }
    }
}