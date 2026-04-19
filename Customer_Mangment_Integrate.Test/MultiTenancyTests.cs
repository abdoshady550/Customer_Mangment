using Customer_Mangment_Integrate.Test.Common;
using System.Net.Http.Headers;

namespace Customer_Mangment_Integrate.Test
{
    public class MultiTenancyTests : TestBase
    {
        public MultiTenancyTests(CustomWebApplicationFactory factory) : base(factory) { }

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
            var http = _factory.CreateIdentityClient();
            http.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId);
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["username"] = AdminEmail,
                ["password"] = AdminPassword,
                ["client_id"] = "customer-management-swagger",
                ["scope"] = "customer_api offline_access roles"
            });
            var response = await http.PostAsync("connect/token", content);
            var body = await response.Content.ReadAsStringAsync();
            return System.Text.Json.JsonDocument.Parse(body)
                .RootElement.GetProperty("access_token").GetString()!;
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

        [Fact]
        public async Task Request_WithoutTenantHeader_Returns400()
        {
            var response = await _factory.CreateClient().GetAsync("api/Customer/get");
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

        [Fact]
        public async Task PasswordGrant_WithoutTenantHeader_Succeeds()
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["username"] = AdminEmail,
                ["password"] = AdminPassword,
                ["client_id"] = "customer-management-swagger",
                ["scope"] = "customer_api offline_access roles"
            });
            var response = await _factory.CreateIdentityClient().PostAsync("connect/token", content);
            var body = await response.Content.ReadAsStringAsync();
            Assert.True(response.IsSuccessStatusCode, $"Expected success but got {(int)response.StatusCode}: {body}");
            Assert.False(string.IsNullOrWhiteSpace(
                System.Text.Json.JsonDocument.Parse(body).RootElement.GetProperty("access_token").GetString()));
        }

        [Fact]
        public async Task RefreshGrant_WithoutTenantHeader_Succeeds()
        {
            var http = _factory.CreateIdentityClient();
            var initialContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["username"] = AdminEmail,
                ["password"] = AdminPassword,
                ["client_id"] = "customer-management-swagger",
                ["scope"] = "customer_api offline_access roles"
            });
            var initialDoc = System.Text.Json.JsonDocument.Parse(await (await http.PostAsync("connect/token", initialContent)).Content.ReadAsStringAsync());
            var refreshToken = initialDoc.RootElement.GetProperty("refresh_token").GetString()!;

            var refreshContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken,
                ["client_id"] = "customer-management-swagger"
            });
            var refreshResponse = await http.PostAsync("connect/token", refreshContent);
            var refreshBody = await refreshResponse.Content.ReadAsStringAsync();
            Assert.True(refreshResponse.IsSuccessStatusCode, $"Refresh failed ({(int)refreshResponse.StatusCode}): {refreshBody}");
        }

        [Fact]
        public async Task Token_IssuedForDefaultTenant_Rejected_WhenUsedWithAlahly()
        {
            var token = await GetAdminTokenAsync();
            var http = _factory.CreateClient();
            http.DefaultRequestHeaders.Add("X-Tenant-Id", TenantAlahly);
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            Assert.Equal(System.Net.HttpStatusCode.Forbidden, (await http.GetAsync("api/Customer/get")).StatusCode);
        }

        [Fact]
        public async Task Token_IssuedForAlahly_Rejected_WhenUsedWithMeccano()
        {
            var token = await GetAdminTokenForTenantAsync(TenantAlahly);
            var http = _factory.CreateClient();
            http.DefaultRequestHeaders.Add("X-Tenant-Id", TenantMeccano);
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            Assert.Equal(System.Net.HttpStatusCode.Forbidden, (await http.GetAsync("api/Customer/get")).StatusCode);
        }

        [Fact]
        public async Task Token_IssuedForTenant_Accepted_WhenUsedWithSameTenant()
        {
            var customers = await CreateApiClient(await GetAdminTokenAsync()).Get2Async(null);
            Assert.NotNull(customers);
        }

        [Fact]
        public async Task Request_WithValidToken_AndNoTenantHeader_Returns400()
        {
            var http = _factory.CreateClient();
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAdminTokenAsync());
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, (await http.GetAsync("api/Customer/get")).StatusCode);
        }

        [Fact]
        public async Task CustomerCreatedInDefaultTenant_NotVisibleInAlahly()
        {
            var defaultClient = CreateApiClient(await GetAdminTokenAsync());
            var alahlyClient = ApiClientForTenant(TenantAlahly, await GetAdminTokenForTenantAsync(TenantAlahly));
            var created = await defaultClient.Add2Async(new CreateCustomerReq { Name = "Demo-Only", Mobile = UniqueMobile(), Adresses = new List<CreateAddressReq>() });
            try { Assert.DoesNotContain(await alahlyClient.Get2Async(null), c => c.Id == created.Id); }
            finally { await CleanupCustomerAsync(defaultClient, created.Id); }
        }

        [Fact]
        public async Task CustomerCreatedInAlahly_NotVisibleInDefaultTenant()
        {
            var defaultClient = CreateApiClient(await GetAdminTokenAsync());
            var alahlyClient = ApiClientForTenant(TenantAlahly, await GetAdminTokenForTenantAsync(TenantAlahly));
            var created = await alahlyClient.Add2Async(new CreateCustomerReq { Name = "Alahly-Only", Mobile = UniqueMobile(), Adresses = new List<CreateAddressReq>() });
            try { Assert.DoesNotContain(await defaultClient.Get2Async(null), c => c.Id == created.Id); }
            finally { await CleanupTenantCustomerAsync(TenantAlahly, created.Id); }
        }

        [Fact]
        public async Task CrossTenantLookupById_Returns404()
        {
            var defaultClient = CreateApiClient(await GetAdminTokenAsync());
            var alahlyClient = ApiClientForTenant(TenantAlahly, await GetAdminTokenForTenantAsync(TenantAlahly));
            var created = await defaultClient.Add2Async(new CreateCustomerReq { Name = "Isolation Target", Mobile = UniqueMobile(), Adresses = new List<CreateAddressReq>() });
            try
            {
                var ex = await Assert.ThrowsAnyAsync<ApiException>(() => alahlyClient.Get2Async(created.Id));
                Assert.Equal(404, ex.StatusCode);
            }
            finally { await CleanupCustomerAsync(defaultClient, created.Id); }
        }

        [Fact]
        public async Task SameMobileInDifferentTenants_BothSucceed()
        {
            var defaultClient = CreateApiClient(await GetAdminTokenAsync());
            var alahlyClient = ApiClientForTenant(TenantAlahly, await GetAdminTokenForTenantAsync(TenantAlahly));
            var mobile = UniqueMobile();
            var dc = await defaultClient.Add2Async(new CreateCustomerReq { Name = "Default", Mobile = mobile, Adresses = new List<CreateAddressReq>() });
            var ac = await alahlyClient.Add2Async(new CreateCustomerReq { Name = "Alahly", Mobile = mobile, Adresses = new List<CreateAddressReq>() });
            try
            {
                Assert.NotEqual(dc.Id, ac.Id);
                Assert.Equal(mobile, dc.Mobile);
                Assert.Equal(mobile, ac.Mobile);
            }
            finally
            {
                await CleanupCustomerAsync(defaultClient, dc.Id);
                await CleanupTenantCustomerAsync(TenantAlahly, ac.Id);
            }
        }

        [Fact]
        public async Task DuplicateMobile_WithinSameTenant_StillConflicts()
        {
            var client = CreateApiClient(await GetAdminTokenAsync());
            var mobile = UniqueMobile();
            var first = await client.Add2Async(new CreateCustomerReq { Name = "First", Mobile = mobile, Adresses = new List<CreateAddressReq>() });
            try
            {
                var ex = await Assert.ThrowsAnyAsync<ApiException>(
                    () => client.Add2Async(new CreateCustomerReq { Name = "Second", Mobile = mobile, Adresses = new List<CreateAddressReq>() }));
                Assert.Equal(409, ex.StatusCode);
            }
            finally { await CleanupCustomerAsync(client, first.Id); }
        }
    }
}
