using Customer_Mangment_Integrate.Test.Common;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Customer_Mangment_Integrate.Test
{
    public class ODataTests : TestBase
    {
        public ODataTests(CustomWebApplicationFactory factory) : base(factory) { }


        private Client CreateODataClient(string? accessToken = null)
        {
            var http = _factory.CreateClient();
            http.DefaultRequestHeaders.Add("X-Tenant-Id", DefaultTenantId);
            if (!string.IsNullOrEmpty(accessToken))
                http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);
            return new Client(http) { BaseUrl = http.BaseAddress?.ToString() ?? "" };
        }

        private static JsonElement ParseODataValue(string json)
        {
            var doc = JsonDocument.Parse(json).RootElement;

            // OData envelope: { "value": [...] }
            if (doc.ValueKind == JsonValueKind.Object &&
                doc.TryGetProperty("value", out var value))
                return value;

            // Plain array returned directly
            if (doc.ValueKind == JsonValueKind.Array)
                return doc;

            // Single object — wrap check failed, return as-is
            return doc;
        }

        private static int ODataCount(string json)
        {
            var doc = JsonDocument.Parse(json).RootElement;

            if (doc.ValueKind == JsonValueKind.Object)
            {
                if (doc.TryGetProperty("@odata.count", out var count))
                    return count.GetInt32();

                if (doc.TryGetProperty("value", out var value))
                    return value.GetArrayLength();
            }

            if (doc.ValueKind == JsonValueKind.Array)
                return doc.GetArrayLength();

            return 0;
        }

        //  /odata/Customers 

        [Fact]
        public async Task OData_GetCustomers_Admin_Returns200WithData()
        {
            var client = CreateODataClient(await GetAdminTokenAsync());
            var created = await CreateTestCustomerAsync(client, "OData Test");
            try
            {
                var raw = await client.GetODataCustomersRawAsync();
                var items = ParseODataValue(raw);

                Assert.Equal(JsonValueKind.Array, items.ValueKind);
                Assert.True(items.GetArrayLength() > 0);
            }
            finally { await CleanupCustomerAsync(client, created.Id); }
        }

        [Fact]
        public async Task OData_GetCustomers_User_Returns200WithData()
        {
            var admin = CreateODataClient(await GetAdminTokenAsync());
            var user = CreateODataClient(await GetUserTokenAsync());
            var created = await CreateTestCustomerAsync(admin, "OData User Test");
            try
            {
                var raw = await user.GetODataCustomersRawAsync();
                var items = ParseODataValue(raw);
                Assert.Equal(JsonValueKind.Array, items.ValueKind);
            }
            finally { await CleanupCustomerAsync(admin, created.Id); }
        }

        [Fact]
        public async Task OData_GetCustomers_WithoutToken_Returns401()
        {
            var client = CreateODataClient();
            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => client.GetODataCustomersRawAsync());
            Assert.Equal(401, ex.StatusCode);
        }

        [Fact]
        public async Task OData_GetCustomers_ContainsCreatedCustomer()
        {
            var client = CreateODataClient(await GetAdminTokenAsync());
            var created = await CreateTestCustomerAsync(client, "OData Find Me");
            try
            {
                var raw = await client.GetODataCustomersRawAsync();
                var items = ParseODataValue(raw);
                var ids = Enumerable.Range(0, items.GetArrayLength())
                    .Select(i => items[i].GetProperty("id").GetString())
                    .ToList();
                Assert.Contains(created.Id.ToString(), ids);
            }
            finally { await CleanupCustomerAsync(client, created.Id); }
        }

        [Fact]
        public async Task OData_GetCustomers_ReturnsExpectedFields()
        {
            var client = CreateODataClient(await GetAdminTokenAsync());
            var created = await CreateTestCustomerAsync(client, "OData Fields");
            try
            {
                var raw = await client.GetODataCustomersRawAsync();
                var items = ParseODataValue(raw);
                var first = Enumerable.Range(0, items.GetArrayLength())
                    .Select(i => items[i])
                    .First(c => c.GetProperty("id").GetString() == created.Id.ToString());

                Assert.True(first.TryGetProperty("id", out _));
                Assert.True(first.TryGetProperty("name", out _));
                Assert.True(first.TryGetProperty("mobile", out _));
                Assert.True(first.TryGetProperty("createdBy", out _));
                Assert.True(first.TryGetProperty("updatedBy", out _));
            }
            finally { await CleanupCustomerAsync(client, created.Id); }
        }

        // $filter 

        [Fact]
        public async Task OData_GetCustomers_Filter_ByName_ReturnsMatchingCustomers()
        {
            var client = CreateODataClient(await GetAdminTokenAsync());
            var uniqueName = $"ODataFilter_{Guid.NewGuid():N}";
            var created = await CreateTestCustomerAsync(client, uniqueName);
            try
            {
                var raw = await client.GetODataCustomersRawAsync(
                    $"$filter=name eq '{uniqueName}'");
                var items = ParseODataValue(raw);

                Assert.True(items.GetArrayLength() >= 1);
                for (int i = 0; i < items.GetArrayLength(); i++)
                    Assert.Equal(uniqueName, items[i].GetProperty("name").GetString());
            }
            finally { await CleanupCustomerAsync(client, created.Id); }
        }

        [Fact]
        public async Task OData_GetCustomers_Filter_ByName_NoMatch_ReturnsEmptyArray()
        {
            var client = CreateODataClient(await GetAdminTokenAsync());
            var raw = await client.GetODataCustomersRawAsync(
                "$filter=name eq 'ABSOLUTELY_NONEXISTENT_XYZ_12345'");
            var items = ParseODataValue(raw);

            Assert.Equal(0, items.GetArrayLength());
        }

        [Fact]
        public async Task OData_GetCustomers_Filter_Contains_ReturnsMatches()
        {
            var client = CreateODataClient(await GetAdminTokenAsync());
            var suffix = Guid.NewGuid().ToString("N")[..8];
            var name = $"ODataContains_{suffix}";
            var created = await CreateTestCustomerAsync(client, name);
            try
            {
                var raw = await client.GetODataCustomersRawAsync(
                    $"$filter=contains(name, '{suffix}')");
                var items = ParseODataValue(raw);

                Assert.True(items.GetArrayLength() >= 1);
                var ids = Enumerable.Range(0, items.GetArrayLength())
                    .Select(i => items[i].GetProperty("id").GetString())
                    .ToList();
                Assert.Contains(created.Id.ToString(), ids);
            }
            finally { await CleanupCustomerAsync(client, created.Id); }
        }


        //  $orderby 

        [Fact]
        public async Task OData_GetCustomers_OrderBy_Name_Asc_ReturnsSortedResults()
        {
            var client = CreateODataClient(await GetAdminTokenAsync());
            var c1 = await CreateTestCustomerAsync(client, "AAA_OData_Sort");
            var c2 = await CreateTestCustomerAsync(client, "ZZZ_OData_Sort");
            try
            {
                var raw = await client.GetODataCustomersRawAsync(
                    "$filter=contains(name,'_OData_Sort')&$orderby=name asc");
                var items = ParseODataValue(raw);

                Assert.True(items.GetArrayLength() >= 2);
                var names = Enumerable.Range(0, items.GetArrayLength())
                    .Select(i => items[i].GetProperty("name").GetString()!)
                    .ToList();

                Assert.True(names.SequenceEqual(names.OrderBy(n => n).ToList()),
                    "Names should be in ascending order");
            }
            finally
            {
                await CleanupCustomerAsync(client, c1.Id);
                await CleanupCustomerAsync(client, c2.Id);
            }
        }

        [Fact]
        public async Task OData_GetCustomers_OrderBy_Name_Desc_ReturnsSortedResults()
        {
            var client = CreateODataClient(await GetAdminTokenAsync());
            var c1 = await CreateTestCustomerAsync(client, "AAA_OData_Desc");
            var c2 = await CreateTestCustomerAsync(client, "ZZZ_OData_Desc");
            try
            {
                var raw = await client.GetODataCustomersRawAsync(
                    "$filter=contains(name,'_OData_Desc')&$orderby=name desc");
                var items = ParseODataValue(raw);

                Assert.True(items.GetArrayLength() >= 2);
                var names = Enumerable.Range(0, items.GetArrayLength())
                    .Select(i => items[i].GetProperty("name").GetString()!)
                    .ToList();

                Assert.True(names.SequenceEqual(names.OrderByDescending(n => n).ToList()),
                    "Names should be in descending order");
            }
            finally
            {
                await CleanupCustomerAsync(client, c1.Id);
                await CleanupCustomerAsync(client, c2.Id);
            }
        }

        //  $top / $skip 

        [Fact]
        public async Task OData_GetCustomers_Top1_ReturnsExactlyOneRecord()
        {
            var client = CreateODataClient(await GetAdminTokenAsync());
            var created = await CreateTestCustomerAsync(client, "OData Top1");
            try
            {
                var raw = await client.GetODataCustomersRawAsync("$top=1");
                var items = ParseODataValue(raw);
                Assert.Equal(1, items.GetArrayLength());
            }
            finally { await CleanupCustomerAsync(client, created.Id); }
        }

        [Fact]
        public async Task OData_GetCustomers_Top2_ReturnsAtMostTwoRecords()
        {
            var client = CreateODataClient(await GetAdminTokenAsync());
            var c1 = await CreateTestCustomerAsync(client, "OData Top2 A");
            var c2 = await CreateTestCustomerAsync(client, "OData Top2 B");
            var c3 = await CreateTestCustomerAsync(client, "OData Top2 C");
            try
            {
                var raw = await client.GetODataCustomersRawAsync("$top=2");
                var items = ParseODataValue(raw);
                Assert.Equal(2, items.GetArrayLength());
            }
            finally
            {
                await CleanupCustomerAsync(client, c1.Id);
                await CleanupCustomerAsync(client, c2.Id);
                await CleanupCustomerAsync(client, c3.Id);
            }
        }

        [Fact]
        public async Task OData_GetCustomers_Skip_ChangesPageOfResults()
        {
            var client = CreateODataClient(await GetAdminTokenAsync());
            var c1 = await CreateTestCustomerAsync(client, "AAA_OData_Skip");
            var c2 = await CreateTestCustomerAsync(client, "BBB_OData_Skip");
            var c3 = await CreateTestCustomerAsync(client, "CCC_OData_Skip");
            try
            {
                var page1Raw = await client.GetODataCustomersRawAsync(
                    "$filter=contains(name,'_OData_Skip')&$orderby=name asc&$top=2&$skip=0");
                var page2Raw = await client.GetODataCustomersRawAsync(
                    "$filter=contains(name,'_OData_Skip')&$orderby=name asc&$top=2&$skip=2");

                var page1 = ParseODataValue(page1Raw);
                var page2 = ParseODataValue(page2Raw);

                Assert.Equal(2, page1.GetArrayLength());
                Assert.Equal(1, page2.GetArrayLength());

                var page1Ids = Enumerable.Range(0, 2)
                    .Select(i => page1[i].GetProperty("id").GetString()).ToHashSet();
                var page2Id = page2[0].GetProperty("id").GetString();

                Assert.DoesNotContain(page2Id, page1Ids);
            }
            finally
            {
                await CleanupCustomerAsync(client, c1.Id);
                await CleanupCustomerAsync(client, c2.Id);
                await CleanupCustomerAsync(client, c3.Id);
            }
        }

        //  $count 

        [Fact]
        public async Task OData_GetCustomers_Count_ReturnsCountInResponse()
        {
            var client = CreateODataClient(await GetAdminTokenAsync());
            var created = await CreateTestCustomerAsync(client, "OData Count");
            try
            {
                var raw = await client.GetODataCustomersRawAsync("$count=true");
                var doc = JsonDocument.Parse(raw).RootElement;

                if (doc.ValueKind == JsonValueKind.Object &&
                    doc.TryGetProperty("@odata.count", out var countProp))
                {
                    Assert.True(countProp.GetInt32() > 0);
                }
                else
                {
                    var items = ParseODataValue(raw);
                    Assert.True(items.GetArrayLength() > 0);
                }
            }
            finally { await CleanupCustomerAsync(client, created.Id); }
        }
        [Fact]
        public async Task OData_GetCustomers_Count_ReflectsFilteredResults()
        {
            var client = CreateODataClient(await GetAdminTokenAsync());
            var uniqueSuffix = Guid.NewGuid().ToString("N")[..8];
            var c1 = await CreateTestCustomerAsync(client, $"ODataCnt_{uniqueSuffix}");
            var c2 = await CreateTestCustomerAsync(client, $"ODataCnt_{uniqueSuffix}B");
            try
            {
                var raw = await client.GetODataCustomersRawAsync(
                    $"$filter=contains(name,'ODataCnt_{uniqueSuffix}')&$count=true");
                var doc = JsonDocument.Parse(raw).RootElement;

                if (doc.ValueKind == JsonValueKind.Object &&
                    doc.TryGetProperty("@odata.count", out var countProp))
                {
                    Assert.Equal(2, countProp.GetInt32());
                }
                else
                {
                    var items = ParseODataValue(raw);
                    Assert.Equal(2, items.GetArrayLength());
                }
            }
            finally
            {
                await CleanupCustomerAsync(client, c1.Id);
                await CleanupCustomerAsync(client, c2.Id);
            }
        }
        [Fact]
        public async Task OData_GetAddresses_Count_ReturnsCountInResponse()
        {
            var client = CreateODataClient(await GetAdminTokenAsync());
            var customer = await CreateTestCustomerAsync(client);
            await AddAddressAsync(client, customer.Id, 1);
            try
            {
                var raw = await client.GetODataAddressesRawAsync(
                    odataQuery: "$count=true",
                    customerId: customer.Id);
                var doc = JsonDocument.Parse(raw).RootElement;

                if (doc.ValueKind == JsonValueKind.Object &&
                    doc.TryGetProperty("@odata.count", out var countProp))
                {
                    Assert.True(countProp.GetInt32() >= 1);
                }
                else
                {
                    var items = ParseODataValue(raw);
                    Assert.True(items.GetArrayLength() >= 1);
                }
            }
            finally { await CleanupCustomerAsync(client, customer.Id); }
        }

        //  combined queries 



        [Fact]
        public async Task OData_GetCustomers_FilterOrderByTop_Combined()
        {
            var client = CreateODataClient(await GetAdminTokenAsync());
            var suffix = Guid.NewGuid().ToString("N")[..8];
            var c1 = await CreateTestCustomerAsync(client, $"Aaa_Combo_{suffix}");
            var c2 = await CreateTestCustomerAsync(client, $"Bbb_Combo_{suffix}");
            var c3 = await CreateTestCustomerAsync(client, $"Ccc_Combo_{suffix}");
            try
            {
                var raw = await client.GetODataCustomersRawAsync(
                    $"$filter=contains(name,'_Combo_{suffix}')&$orderby=name desc&$top=2");
                var items = ParseODataValue(raw);

                Assert.Equal(2, items.GetArrayLength());
                Assert.Equal($"Ccc_Combo_{suffix}", items[0].GetProperty("name").GetString());
                Assert.Equal($"Bbb_Combo_{suffix}", items[1].GetProperty("name").GetString());
            }
            finally
            {
                await CleanupCustomerAsync(client, c1.Id);
                await CleanupCustomerAsync(client, c2.Id);
                await CleanupCustomerAsync(client, c3.Id);
            }
        }

        //  /odata/Addresses 

        [Fact]
        public async Task OData_GetAddresses_Admin_Returns200WithData()
        {
            var client = CreateODataClient(await GetAdminTokenAsync());
            var customer = await CreateTestCustomerAsync(client);
            await AddAddressAsync(client, customer.Id, 1);
            try
            {
                var raw = await client.GetODataAddressesRawAsync();
                var items = ParseODataValue(raw);
                Assert.True(items.GetArrayLength() > 0);
            }
            finally { await CleanupCustomerAsync(client, customer.Id); }
        }

        [Fact]
        public async Task OData_GetAddresses_WithoutToken_Returns401()
        {
            var client = CreateODataClient();
            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => client.GetODataAddressesRawAsync());
            Assert.Equal(401, ex.StatusCode);
        }

        [Fact]
        public async Task OData_GetAddresses_FilterByCustomerId_ReturnsOnlyThatCustomer()
        {
            var client = CreateODataClient(await GetAdminTokenAsync());
            var customer = await CreateTestCustomerAsync(client);
            var addr = await AddAddressAsync(client, customer.Id, 1);
            try
            {
                var raw = await client.GetODataAddressesRawAsync(customerId: customer.Id);
                var items = ParseODataValue(raw);

                Assert.True(items.GetArrayLength() >= 1);
                for (int i = 0; i < items.GetArrayLength(); i++)
                    Assert.Equal(customer.Id.ToString(),
                        items[i].GetProperty("customerId").GetString());
            }
            finally { await CleanupCustomerAsync(client, customer.Id); }
        }

        [Fact]
        public async Task OData_GetAddresses_Filter_ByType_ReturnsOnlyMatchingType()
        {
            var client = CreateODataClient(await GetAdminTokenAsync());
            var customer = await CreateTestCustomerAsync(client);
            await AddAddressAsync(client, customer.Id, 1); // Home
            try
            {
                var raw = await client.GetODataAddressesRawAsync(
                    odataQuery: "$filter=type eq 1",
                    customerId: customer.Id);
                var items = ParseODataValue(raw);

                Assert.True(items.GetArrayLength() >= 1);
                for (int i = 0; i < items.GetArrayLength(); i++)
                    Assert.Equal(1, items[i].GetProperty("type").GetInt32());
            }
            finally { await CleanupCustomerAsync(client, customer.Id); }
        }



        [Fact]
        public async Task OData_GetAddresses_Top1_ReturnsOneAddress()
        {
            var client = CreateODataClient(await GetAdminTokenAsync());
            var customer = await CreateTestCustomerAsync(client);
            await AddAddressAsync(client, customer.Id, 1);
            try
            {
                var raw = await client.GetODataAddressesRawAsync(
                    odataQuery: "$top=1",
                    customerId: customer.Id);
                var items = ParseODataValue(raw);
                Assert.Equal(1, items.GetArrayLength());
            }
            finally { await CleanupCustomerAsync(client, customer.Id); }
        }



        [Fact]
        public async Task OData_GetAddresses_OrderBy_Type_Asc_ReturnsSorted()
        {
            var client = CreateODataClient(await GetAdminTokenAsync());
            var customer = await CreateTestCustomerAsync(client);
            await AddAddressAsync(client, customer.Id, 2);
            await AddAddressAsync(client, customer.Id, 3);

            try
            {
                var raw = await client.GetODataAddressesRawAsync(
                    odataQuery: "$orderby=type asc",
                    customerId: customer.Id);
                var items = ParseODataValue(raw);

                var types = Enumerable.Range(0, items.GetArrayLength())
                    .Select(i => items[i].GetProperty("type").GetInt32())
                    .ToList();

                Assert.True(types.SequenceEqual(types.OrderBy(t => t).ToList()),
                    "Types should be in ascending order");
            }
            finally { await CleanupCustomerAsync(client, customer.Id); }
        }

        //  cross-cutting 

        [Fact]
        public async Task OData_GetCustomers_AfterDelete_CustomerNoLongerAppears()
        {
            var client = CreateODataClient(await GetAdminTokenAsync());
            var created = await CreateTestCustomerAsync(client, "OData Delete Check");

            await client.Delete2Async(created.Id);

            var raw = await client.GetODataCustomersRawAsync();
            var items = ParseODataValue(raw);
            var ids = Enumerable.Range(0, items.GetArrayLength())
                .Select(i => items[i].GetProperty("id").GetString())
                .ToList();
            Assert.DoesNotContain(created.Id.ToString(), ids);
        }

        [Fact]
        public async Task OData_GetCustomers_AfterUpdate_ReflectsNewName()
        {
            var client = CreateODataClient(await GetAdminTokenAsync());
            var uniqueName = $"ODataBefore_{Guid.NewGuid():N}";
            var newName = $"ODataAfter_{Guid.NewGuid():N}";
            var created = await CreateTestCustomerAsync(client, uniqueName);
            try
            {
                await client.Update2Async(created.Id, new UpdateCustomerReq
                {
                    Name = newName,
                    Mobile = UniqueMobile()
                });

                var raw = await client.GetODataCustomersRawAsync(
                    $"$filter=name eq '{newName}'");
                var items = ParseODataValue(raw);

                Assert.Equal(1, items.GetArrayLength());
                Assert.Equal(created.Id.ToString(), items[0].GetProperty("id").GetString());
            }
            finally { await CleanupCustomerAsync(client, created.Id); }
        }

        [Fact]
        public async Task OData_GetAddresses_AfterDelete_AddressNoLongerAppears()
        {
            var client = CreateODataClient(await GetAdminTokenAsync());
            var customer = await CreateTestCustomerAsync(client);
            var addr = await AddAddressAsync(client, customer.Id, 1);
            try
            {
                await client.DeleteAsync(addr.Id);

                var raw = await client.GetODataAddressesRawAsync(customerId: customer.Id);
                var items = ParseODataValue(raw);
                var ids = Enumerable.Range(0, items.GetArrayLength())
                    .Select(i => items[i].GetProperty("id").GetString())
                    .ToList();
                Assert.DoesNotContain(addr.Id.ToString(), ids);
            }
            finally { await CleanupCustomerAsync(client, customer.Id); }
        }

        [Fact]
        public async Task OData_GetCustomers_TenantIsolation_OtherTenantNotVisible()
        {
            var defaultClient = CreateODataClient(await GetAdminTokenAsync());
            var created = await CreateTestCustomerAsync(defaultClient, "OData Isolation");
            try
            {
                var alahlyToken = await GetTokenForTenantAsync("alahly");
                var alahlyHttp = _factory.CreateClient();
                alahlyHttp.DefaultRequestHeaders.Add("X-Tenant-Id", "alahly");
                alahlyHttp.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", alahlyToken);
                var alahlyClient = new Client(alahlyHttp)
                {
                    BaseUrl = alahlyHttp.BaseAddress?.ToString() ?? ""
                };

                var raw = await alahlyClient.GetODataCustomersRawAsync();
                var items = ParseODataValue(raw);
                var ids = Enumerable.Range(0, items.GetArrayLength())
                    .Select(i => items[i].GetProperty("id").GetString())
                    .ToList();
                Assert.DoesNotContain(created.Id.ToString(), ids);
            }
            finally { await CleanupCustomerAsync(defaultClient, created.Id); }
        }


        private async Task<string> GetTokenForTenantAsync(string tenantId)
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
            return JsonDocument.Parse(body).RootElement
                .GetProperty("access_token").GetString()!;
        }
    }
}