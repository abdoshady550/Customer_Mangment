using Customer_Mangment_Integrate.Test.Common;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Customer_Mangment_Integrate.Test
{

    public class GraphQLTests : TestBase
    {
        private const string GraphQLEndpoint = "/graphQL";

        public GraphQLTests(CustomWebApplicationFactory factory) : base(factory) { }


        private HttpClient CreateGraphQLClient(string? accessToken = null)
        {
            var http = _factory.CreateClient();
            http.DefaultRequestHeaders.Add("X-Tenant-Id", DefaultTenantId);
            if (!string.IsNullOrEmpty(accessToken))
                http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);
            return http;
        }

        private static StringContent GraphQLContent(string query, object? variables = null)
        {
            var body = variables is null
                ? JsonSerializer.Serialize(new { query })
                : JsonSerializer.Serialize(new { query, variables });
            return new StringContent(body, Encoding.UTF8, "application/json");
        }

        private async Task<JsonElement> ExecuteAsync(HttpClient http, string query, object? variables = null)
        {
            var response = await http.PostAsync(GraphQLEndpoint, GraphQLContent(query, variables));
            var json = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(json).RootElement;
        }

        private static bool HasErrors(JsonElement root) =>
            root.TryGetProperty("errors", out var errors) && errors.GetArrayLength() > 0;

        private static string FirstErrorMessage(JsonElement root) =>
            root.GetProperty("errors")[0].GetProperty("message").GetString() ?? "";

        private static string FirstErrorCode(JsonElement root) =>
            root.TryGetProperty("errors", out var errors) &&
            errors[0].TryGetProperty("extensions", out var ext) &&
            ext.TryGetProperty("code", out var code)
                ? code.GetString() ?? ""
                : "";

        //  GetCustomers query 

        private const string GetCustomersQuery = @"
            query GetCustomers($customerId: UUID) {
                getCustomers(customerId: $customerId) {
                    id
                    name
                    mobile
                    createdBy
                    updatedBy
                    addresses {
                        id
                        customerId
                        type
                        value
                    }
                }
            }";

        [Fact]
        public async Task GraphQL_GetCustomers_Admin_ReturnsData()
        {
            var token = await GetAdminTokenAsync();
            var restClient = CreateApiClient(token);
            var created = await CreateTestCustomerAsync(restClient, "GQL Read Test");

            try
            {
                var http = CreateGraphQLClient(token);
                var result = await ExecuteAsync(http, GetCustomersQuery);

                Assert.False(HasErrors(result), $"GraphQL errors: {result}");
                var customers = result.GetProperty("data").GetProperty("getCustomers");
                Assert.True(customers.GetArrayLength() > 0);
            }
            finally { await CleanupCustomerAsync(restClient, created.Id); }
        }

        [Fact]
        public async Task GraphQL_GetCustomers_User_ReturnsData()
        {
            var admin = CreateApiClient(await GetAdminTokenAsync());
            var created = await CreateTestCustomerAsync(admin, "GQL User Read");

            try
            {
                var http = CreateGraphQLClient(await GetUserTokenAsync());
                var result = await ExecuteAsync(http, GetCustomersQuery);

                Assert.False(HasErrors(result), $"GraphQL errors: {result}");
                var customers = result.GetProperty("data").GetProperty("getCustomers");
                Assert.True(customers.GetArrayLength() > 0);
            }
            finally { await CleanupCustomerAsync(admin, created.Id); }
        }

        [Fact]
        public async Task GraphQL_GetCustomers_WithCustomerId_ReturnsSingleMatch()
        {
            var token = await GetAdminTokenAsync();
            var restClient = CreateApiClient(token);
            var created = await CreateTestCustomerAsync(restClient, "GQL ById");

            try
            {
                var http = CreateGraphQLClient(token);
                var result = await ExecuteAsync(http, GetCustomersQuery,
                    new { customerId = created.Id });

                Assert.False(HasErrors(result), $"GraphQL errors: {result}");
                var customers = result.GetProperty("data").GetProperty("getCustomers");
                Assert.Equal(1, customers.GetArrayLength());
                Assert.Equal(created.Id.ToString(), customers[0].GetProperty("id").GetString());
                Assert.Equal("GQL ById", customers[0].GetProperty("name").GetString());
            }
            finally { await CleanupCustomerAsync(restClient, created.Id); }
        }

        [Fact]
        public async Task GraphQL_GetCustomers_WithCustomerId_IncludesAddressFields()
        {
            var token = await GetAdminTokenAsync();
            var restClient = CreateApiClient(token);
            var created = await restClient.Add2Async(new CreateCustomerReq
            {
                Name = "GQL Addr Fields",
                Mobile = UniqueMobile(),
                Adresses = new List<CreateAddressReq>
                {
                    new CreateAddressReq { Type = 1, Value = "GraphQL Address" }
                }
            });

            try
            {
                var http = CreateGraphQLClient(token);
                var result = await ExecuteAsync(http, GetCustomersQuery,
                    new { customerId = created.Id });

                Assert.False(HasErrors(result), $"GraphQL errors: {result}");
                var customer = result.GetProperty("data").GetProperty("getCustomers")[0];
                var addresses = customer.GetProperty("addresses");
                Assert.True(addresses.GetArrayLength() > 0);

                var addr = addresses[0];
                Assert.True(addr.TryGetProperty("id", out _));
                Assert.True(addr.TryGetProperty("customerId", out _));
                Assert.True(addr.TryGetProperty("type", out _));
                Assert.True(addr.TryGetProperty("value", out _));
            }
            finally { await CleanupCustomerAsync(restClient, created.Id); }
        }

        [Fact]
        public async Task GraphQL_GetCustomers_WithUnknownId_ReturnsError()
        {
            var http = CreateGraphQLClient(await GetAdminTokenAsync());
            var result = await ExecuteAsync(http, GetCustomersQuery,
                new { customerId = Guid.NewGuid() });

            Assert.True(HasErrors(result), "Expected GraphQL error for unknown customer ID");
        }

        [Fact]
        public async Task GraphQL_GetCustomers_WithoutToken_ReturnsUnauthorizedError()
        {
            var http = CreateGraphQLClient();
            var result = await ExecuteAsync(http, GetCustomersQuery);

            Assert.True(HasErrors(result), "Expected auth error without token");
        }

        [Fact]
        public async Task GraphQL_GetCustomers_ReturnsCorrectFields()
        {
            var token = await GetAdminTokenAsync();
            var restClient = CreateApiClient(token);
            var mobile = UniqueMobile();
            var created = await CreateTestCustomerAsync(restClient, "GQL Fields", mobile);

            try
            {
                var http = CreateGraphQLClient(token);
                var result = await ExecuteAsync(http, GetCustomersQuery,
                    new { customerId = created.Id });

                var customer = result.GetProperty("data").GetProperty("getCustomers")[0];
                Assert.Equal(created.Id.ToString(), customer.GetProperty("id").GetString());
                Assert.Equal("GQL Fields", customer.GetProperty("name").GetString());
                Assert.Equal(mobile, customer.GetProperty("mobile").GetString());
                Assert.Equal(AdminEmail, customer.GetProperty("createdBy").GetString());
                Assert.Equal(AdminEmail, customer.GetProperty("updatedBy").GetString());
            }
            finally { await CleanupCustomerAsync(restClient, created.Id); }
        }

        [Fact]
        public async Task GraphQL_GetCustomers_AfterCreate_ContainsNewCustomer()
        {
            var token = await GetAdminTokenAsync();
            var restClient = CreateApiClient(token);
            var created = await CreateTestCustomerAsync(restClient, "GQL Contains");

            try
            {
                var http = CreateGraphQLClient(token);
                var result = await ExecuteAsync(http, GetCustomersQuery);

                Assert.False(HasErrors(result));
                var customers = result.GetProperty("data").GetProperty("getCustomers");
                var ids = Enumerable.Range(0, customers.GetArrayLength())
                    .Select(i => customers[i].GetProperty("id").GetString())
                    .ToList();
                Assert.Contains(created.Id.ToString(), ids);
            }
            finally { await CleanupCustomerAsync(restClient, created.Id); }
        }

        //  GetAddresses query 

        private const string GetAddressesQuery = @"
            query GetAddresses($customerId: UUID, $addressId: UUID) {
                getAddresses(customerId: $customerId, addressId: $addressId) {
                    id
                    customerId
                    type
                    value
                }
            }";

        [Fact]
        public async Task GraphQL_GetAddresses_Admin_ReturnsAll()
        {
            var token = await GetAdminTokenAsync();
            var restClient = CreateApiClient(token);
            var customer = await CreateTestCustomerAsync(restClient);
            await AddAddressAsync(restClient, customer.Id, 1);

            try
            {
                var http = CreateGraphQLClient(token);
                var result = await ExecuteAsync(http, GetAddressesQuery);

                Assert.False(HasErrors(result), $"GraphQL errors: {result}");
                var addresses = result.GetProperty("data").GetProperty("getAddresses");
                Assert.True(addresses.GetArrayLength() > 0);
            }
            finally { await CleanupCustomerAsync(restClient, customer.Id); }
        }

        [Fact]
        public async Task GraphQL_GetAddresses_FilterByCustomerId_ReturnsOnlyThatCustomer()
        {
            var token = await GetAdminTokenAsync();
            var restClient = CreateApiClient(token);
            var customer = await CreateTestCustomerAsync(restClient);
            var addr = await AddAddressAsync(restClient, customer.Id, 1);

            try
            {
                var http = CreateGraphQLClient(token);
                var result = await ExecuteAsync(http, GetAddressesQuery,
                    new { customerId = customer.Id });

                Assert.False(HasErrors(result), $"GraphQL errors: {result}");
                var addresses = result.GetProperty("data").GetProperty("getAddresses");

                for (int i = 0; i < addresses.GetArrayLength(); i++)
                    Assert.Equal(customer.Id.ToString(),
                        addresses[i].GetProperty("customerId").GetString());
            }
            finally { await CleanupCustomerAsync(restClient, customer.Id); }
        }

        [Fact]
        public async Task GraphQL_GetAddresses_FilterByAddressId_ReturnsSingleAddress()
        {
            var token = await GetAdminTokenAsync();
            var restClient = CreateApiClient(token);
            var customer = await CreateTestCustomerAsync(restClient);
            var addr = await AddAddressAsync(restClient, customer.Id, 1);

            try
            {
                var http = CreateGraphQLClient(token);
                var result = await ExecuteAsync(http, GetAddressesQuery,
                    new { customerId = customer.Id, addressId = addr.Id });

                Assert.False(HasErrors(result));
                var addresses = result.GetProperty("data").GetProperty("getAddresses");
                Assert.Equal(1, addresses.GetArrayLength());
                Assert.Equal(addr.Id.ToString(), addresses[0].GetProperty("id").GetString());
            }
            finally { await CleanupCustomerAsync(restClient, customer.Id); }
        }

        [Fact]
        public async Task GraphQL_GetAddresses_WithoutToken_ReturnsUnauthorizedError()
        {
            var http = CreateGraphQLClient();
            var result = await ExecuteAsync(http, GetAddressesQuery);

            Assert.True(HasErrors(result), "Expected auth error without token");
        }

        [Fact]
        public async Task GraphQL_GetAddresses_User_CanRead()
        {
            var admin = CreateApiClient(await GetAdminTokenAsync());
            var customer = await CreateTestCustomerAsync(admin);
            await AddAddressAsync(admin, customer.Id, 1);

            try
            {
                var http = CreateGraphQLClient(await GetUserTokenAsync());
                var result = await ExecuteAsync(http, GetAddressesQuery,
                    new { customerId = customer.Id });

                Assert.False(HasErrors(result));
            }
            finally { await CleanupCustomerAsync(admin, customer.Id); }
        }

        [Fact]
        public async Task GraphQL_GetAddresses_ReturnsCorrectFields()
        {
            var token = await GetAdminTokenAsync();
            var restClient = CreateApiClient(token);
            var customer = await CreateTestCustomerAsync(restClient);
            var addr = await restClient.AddAsync(customer.Id,
                new AddAddressReq { Type = 2, Value = "Work Address" });

            try
            {
                var http = CreateGraphQLClient(token);
                var result = await ExecuteAsync(http, GetAddressesQuery,
                    new { customerId = customer.Id, addressId = addr.Id });

                Assert.False(HasErrors(result));
                var first = result.GetProperty("data").GetProperty("getAddresses")[0];
                Assert.Equal(addr.Id.ToString(), first.GetProperty("id").GetString());
                Assert.Equal(customer.Id.ToString(), first.GetProperty("customerId").GetString());
                Assert.Equal(2, first.GetProperty("type").GetInt32());
                Assert.Equal("Work Address", first.GetProperty("value").GetString());
            }
            finally { await CleanupCustomerAsync(restClient, customer.Id); }
        }

        //  CreateCustomer mutation 

        private const string CreateCustomerMutation = @"
            mutation CreateCustomer($input: CreateCustomerInput!) {
                createCustomer(input: $input) {
                    id
                    name
                    mobile
                    createdBy
                    updatedBy
                    addresses {
                        id
                        customerId
                        type
                        value
                    }
                }
            }";

        [Fact]
        public async Task GraphQL_CreateCustomer_Admin_ReturnsCreatedCustomer()
        {
            var token = await GetAdminTokenAsync();
            var http = CreateGraphQLClient(token);
            var mobile = UniqueMobile();

            var result = await ExecuteAsync(http, CreateCustomerMutation,
                new
                {
                    input = new { name = "GQL Create", mobile, addresses = Array.Empty<object>() }
                });

            Assert.False(HasErrors(result), $"GraphQL errors: {result}");
            var customer = result.GetProperty("data").GetProperty("createCustomer");
            var id = Guid.Parse(customer.GetProperty("id").GetString()!);

            try
            {
                Assert.NotEqual(Guid.Empty, id);
                Assert.Equal("GQL Create", customer.GetProperty("name").GetString());
                Assert.Equal(mobile, customer.GetProperty("mobile").GetString());
                Assert.Equal(AdminEmail, customer.GetProperty("createdBy").GetString());
                Assert.Equal(AdminEmail, customer.GetProperty("updatedBy").GetString());
            }
            finally { await CleanupCustomerAsync(CreateApiClient(token), id); }
        }

        [Fact]
        public async Task GraphQL_CreateCustomer_User_ReturnsCreatedCustomer_WithUserEmail()
        {
            var userToken = await GetUserTokenAsync();
            var http = CreateGraphQLClient(userToken);
            var mobile = UniqueMobile();

            var result = await ExecuteAsync(http, CreateCustomerMutation,
                new
                {
                    input = new { name = "GQL User Create", mobile, addresses = Array.Empty<object>() }
                });

            Assert.False(HasErrors(result), $"GraphQL errors: {result}");
            var customer = result.GetProperty("data").GetProperty("createCustomer");
            var id = Guid.Parse(customer.GetProperty("id").GetString()!);

            try
            {
                Assert.Equal(UserEmail, customer.GetProperty("createdBy").GetString());
            }
            finally { await CleanupCustomerAsync(CreateApiClient(await GetAdminTokenAsync()), id); }
        }

        [Fact]
        public async Task GraphQL_CreateCustomer_WithAddresses_ReturnsAddressesInResponse()
        {
            var token = await GetAdminTokenAsync();
            var http = CreateGraphQLClient(token);

            var result = await ExecuteAsync(http, CreateCustomerMutation,
                new
                {
                    input = new
                    {
                        name = "GQL With Addr",
                        mobile = UniqueMobile(),
                        addresses = new[]
                        {
                            new { type = "HOME", value = "Cairo" },
                            new { type = "WORK", value = "Giza" }
                        }
                    }
                });

            Assert.False(HasErrors(result), $"GraphQL errors: {result}");
            var customer = result.GetProperty("data").GetProperty("createCustomer");
            var id = Guid.Parse(customer.GetProperty("id").GetString()!);

            try
            {
                var addresses = customer.GetProperty("addresses");
                Assert.Equal(2, addresses.GetArrayLength());
            }
            finally { await CleanupCustomerAsync(CreateApiClient(token), id); }
        }

        [Fact]
        public async Task GraphQL_CreateCustomer_WithoutToken_ReturnsUnauthorizedError()
        {
            var http = CreateGraphQLClient();
            var result = await ExecuteAsync(http, CreateCustomerMutation,
                new
                {
                    input = new { name = "GQL Anon", mobile = UniqueMobile(), addresses = Array.Empty<object>() }
                });

            Assert.True(HasErrors(result), "Expected auth error without token");
            var code = FirstErrorCode(result);
            Assert.True(code == "UNAUTHORIZED" || FirstErrorMessage(result).Contains("auth", StringComparison.OrdinalIgnoreCase)
                || FirstErrorMessage(result).Contains("Not authenticated", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GraphQL_CreateCustomer_DuplicateMobile_ReturnsConflictError()
        {
            var token = await GetAdminTokenAsync();
            var http = CreateGraphQLClient(token);
            var mobile = UniqueMobile();

            var first = await ExecuteAsync(http, CreateCustomerMutation,
                new { input = new { name = "GQL First", mobile, addresses = Array.Empty<object>() } });

            Assert.False(HasErrors(first), $"First creation should succeed: {first}");
            var id = Guid.Parse(first.GetProperty("data").GetProperty("createCustomer")
                .GetProperty("id").GetString()!);

            try
            {
                var second = await ExecuteAsync(http, CreateCustomerMutation,
                    new { input = new { name = "GQL Second", mobile, addresses = Array.Empty<object>() } });

                Assert.True(HasErrors(second), "Expected conflict error for duplicate mobile");
            }
            finally { await CleanupCustomerAsync(CreateApiClient(token), id); }
        }

        [Fact]
        public async Task GraphQL_CreateCustomer_CreatedCustomer_AppearsInGetCustomers()
        {
            var token = await GetAdminTokenAsync();
            var http = CreateGraphQLClient(token);
            var mobile = UniqueMobile();

            var createResult = await ExecuteAsync(http, CreateCustomerMutation,
                new { input = new { name = "GQL Visibility", mobile, addresses = Array.Empty<object>() } });

            Assert.False(HasErrors(createResult), $"GraphQL errors: {createResult}");
            var id = Guid.Parse(createResult.GetProperty("data").GetProperty("createCustomer")
                .GetProperty("id").GetString()!);

            try
            {
                var getResult = await ExecuteAsync(http, GetCustomersQuery, new { customerId = id });
                Assert.False(HasErrors(getResult));
                var customers = getResult.GetProperty("data").GetProperty("getCustomers");
                Assert.Equal(1, customers.GetArrayLength());
                Assert.Equal(id.ToString(), customers[0].GetProperty("id").GetString());
            }
            finally { await CleanupCustomerAsync(CreateApiClient(token), id); }
        }

        [Fact]
        public async Task GraphQL_CreateCustomer_IdIsNonEmpty()
        {
            var token = await GetAdminTokenAsync();
            var http = CreateGraphQLClient(token);

            var result = await ExecuteAsync(http, CreateCustomerMutation,
                new { input = new { name = "GQL NonEmpty Id", mobile = UniqueMobile(), addresses = Array.Empty<object>() } });

            Assert.False(HasErrors(result), $"GraphQL errors: {result}");
            var id = Guid.Parse(result.GetProperty("data").GetProperty("createCustomer")
                .GetProperty("id").GetString()!);

            try { Assert.NotEqual(Guid.Empty, id); }
            finally { await CleanupCustomerAsync(CreateApiClient(token), id); }
        }

        // ── AddAddress mutation ────

        private const string AddAddressMutation = @"
            mutation AddAddress($input: AddAddressInput!) {
                addAddress(input: $input) {
                    id
                    customerId
                    type
                    value
                }
            }";

        [Fact]
        public async Task GraphQL_AddAddress_Admin_ReturnsAddressDto()
        {
            var token = await GetAdminTokenAsync();
            var restClient = CreateApiClient(token);
            var customer = await CreateTestCustomerAsync(restClient);

            try
            {
                var http = CreateGraphQLClient(token);
                var result = await ExecuteAsync(http, AddAddressMutation,
                    new
                    {
                        input = new { customerId = customer.Id, type = "HOME", value = "GQL Address" }
                    });

                Assert.False(HasErrors(result), $"GraphQL errors: {result}");
                var addr = result.GetProperty("data").GetProperty("addAddress");
                Assert.NotEqual(Guid.Empty.ToString(), addr.GetProperty("id").GetString());
                Assert.Equal(customer.Id.ToString(), addr.GetProperty("customerId").GetString());
                Assert.Equal("GQL Address", addr.GetProperty("value").GetString());
            }
            finally { await CleanupCustomerAsync(restClient, customer.Id); }
        }

        [Fact]
        public async Task GraphQL_AddAddress_AppearsInGetAddresses()
        {
            var token = await GetAdminTokenAsync();
            var restClient = CreateApiClient(token);
            var customer = await CreateTestCustomerAsync(restClient);

            try
            {
                var http = CreateGraphQLClient(token);
                var addResult = await ExecuteAsync(http, AddAddressMutation,
                    new
                    {
                        input = new { customerId = customer.Id, type = "HOME", value = "Verify GQL" }
                    });

                Assert.False(HasErrors(addResult), $"AddAddress errors: {addResult}");
                var addedId = addResult.GetProperty("data").GetProperty("addAddress")
                    .GetProperty("id").GetString();

                var getResult = await ExecuteAsync(http, GetAddressesQuery,
                    new { customerId = customer.Id });
                Assert.False(HasErrors(getResult));

                var addresses = getResult.GetProperty("data").GetProperty("getAddresses");
                var ids = Enumerable.Range(0, addresses.GetArrayLength())
                    .Select(i => addresses[i].GetProperty("id").GetString())
                    .ToList();

                Assert.Contains(addedId, ids);
            }
            finally { await CleanupCustomerAsync(restClient, customer.Id); }
        }

        [Fact]
        public async Task GraphQL_AddAddress_WithoutToken_ReturnsUnauthorizedError()
        {
            var http = CreateGraphQLClient();
            var result = await ExecuteAsync(http, AddAddressMutation,
                new
                {
                    input = new { customerId = Guid.NewGuid(), type = "HOME", value = "Test" }
                });

            Assert.True(HasErrors(result), "Expected auth error without token");
        }

        [Fact]
        public async Task GraphQL_AddAddress_User_ReturnsForbiddenOrUnauthorized()
        {
            var admin = CreateApiClient(await GetAdminTokenAsync());
            var customer = await CreateTestCustomerAsync(admin);

            try
            {
                var http = CreateGraphQLClient(await GetUserTokenAsync());
                var result = await ExecuteAsync(http, AddAddressMutation,
                    new
                    {
                        input = new { customerId = customer.Id, type = "HOME", value = "Should Fail" }
                    });

                // GraphQL returns 200 with errors for authorization failures
                Assert.True(HasErrors(result), "Expected auth error for User role");
            }
            finally { await CleanupCustomerAsync(admin, customer.Id); }
        }

        [Fact]
        public async Task GraphQL_AddAddress_NonExistentCustomer_ReturnsError()
        {
            var http = CreateGraphQLClient(await GetAdminTokenAsync());
            var result = await ExecuteAsync(http, AddAddressMutation,
                new
                {
                    input = new { customerId = Guid.NewGuid(), type = "HOME", value = "Ghost" }
                });

            Assert.True(HasErrors(result), "Expected error for non-existent customer");
        }

        [Fact]
        public async Task GraphQL_AddAddress_ReturnsCorrectType()
        {
            var token = await GetAdminTokenAsync();
            var restClient = CreateApiClient(token);
            var customer = await CreateTestCustomerAsync(restClient);

            try
            {
                var http = CreateGraphQLClient(token);
                var result = await ExecuteAsync(http, AddAddressMutation,
                    new
                    {
                        input = new { customerId = customer.Id, type = "WORK", value = "Work GQL" }
                    });

                Assert.False(HasErrors(result), $"GraphQL errors: {result}");
                // Type is returned as integer in the DTO (AdressType enum)
                var typeVal = result.GetProperty("data").GetProperty("addAddress").GetProperty("type");
                Assert.True(typeVal.ValueKind == JsonValueKind.Number || typeVal.ValueKind == JsonValueKind.String,
                    "type field should be present");
            }
            finally { await CleanupCustomerAsync(restClient, customer.Id); }
        }

        //  End-to-end GraphQL scenario 

        [Fact]
        public async Task GraphQL_FullScenario_CreateCustomer_AddAddress_ThenQuery()
        {
            var token = await GetAdminTokenAsync();
            var http = CreateGraphQLClient(token);

            // Step 1: Create customer via GraphQL
            var createResult = await ExecuteAsync(http, CreateCustomerMutation,
                new
                {
                    input = new
                    {
                        name = "GQL E2E Customer",
                        mobile = UniqueMobile(),
                        addresses = Array.Empty<object>()
                    }
                });

            Assert.False(HasErrors(createResult), $"Create errors: {createResult}");
            var customerId = Guid.Parse(createResult.GetProperty("data")
                .GetProperty("createCustomer").GetProperty("id").GetString()!);

            try
            {
                // Step 2: Add address via GraphQL
                var addResult = await ExecuteAsync(http, AddAddressMutation,
                    new
                    {
                        input = new { customerId, type = "HOME", value = "E2E Home" }
                    });

                Assert.False(HasErrors(addResult), $"Add address errors: {addResult}");
                var addressId = addResult.GetProperty("data").GetProperty("addAddress")
                    .GetProperty("id").GetString();

                // Step 3: Query customer – should include the new address
                var queryResult = await ExecuteAsync(http, GetCustomersQuery,
                    new { customerId });

                Assert.False(HasErrors(queryResult), $"Query errors: {queryResult}");
                var customer = queryResult.GetProperty("data").GetProperty("getCustomers")[0];
                var addresses = customer.GetProperty("addresses");

                Assert.True(addresses.GetArrayLength() > 0);
                var addrIds = Enumerable.Range(0, addresses.GetArrayLength())
                    .Select(i => addresses[i].GetProperty("id").GetString())
                    .ToList();
                Assert.Contains(addressId, addrIds);

                // Step 4: Query addresses directly
                var addrQueryResult = await ExecuteAsync(http, GetAddressesQuery,
                    new { customerId, addressId = Guid.Parse(addressId!) });

                Assert.False(HasErrors(addrQueryResult));
                var fetchedAddr = addrQueryResult.GetProperty("data").GetProperty("getAddresses");
                Assert.Equal(1, fetchedAddr.GetArrayLength());
                Assert.Equal("E2E Home", fetchedAddr[0].GetProperty("value").GetString());
            }
            finally { await CleanupCustomerAsync(CreateApiClient(token), customerId); }
        }

        [Fact]
        public async Task GraphQL_UserCreatesCustomer_AdminAddsAddress_QueryReturnsAll()
        {
            var adminToken = await GetAdminTokenAsync();
            var userToken = await GetUserTokenAsync();

            var userHttp = CreateGraphQLClient(userToken);
            var adminHttp = CreateGraphQLClient(adminToken);

            // User creates customer
            var createResult = await ExecuteAsync(userHttp, CreateCustomerMutation,
                new
                {
                    input = new
                    {
                        name = "GQL Cross Role",
                        mobile = UniqueMobile(),
                        addresses = Array.Empty<object>()
                    }
                });

            Assert.False(HasErrors(createResult), $"Create errors: {createResult}");
            var customerId = Guid.Parse(createResult.GetProperty("data")
                .GetProperty("createCustomer").GetProperty("id").GetString()!);

            try
            {
                Assert.Equal(UserEmail,
                    createResult.GetProperty("data").GetProperty("createCustomer")
                        .GetProperty("createdBy").GetString());

                // Admin adds address
                var addResult = await ExecuteAsync(adminHttp, AddAddressMutation,
                    new { input = new { customerId, type = "HOME", value = "Admin Added" } });

                Assert.False(HasErrors(addResult), $"Add address errors: {addResult}");

                // Both can query
                var userQuery = await ExecuteAsync(userHttp, GetCustomersQuery, new { customerId });
                var adminQuery = await ExecuteAsync(adminHttp, GetCustomersQuery, new { customerId });

                Assert.False(HasErrors(userQuery));
                Assert.False(HasErrors(adminQuery));
                Assert.Equal(
                    userQuery.GetProperty("data").GetProperty("getCustomers")[0]
                        .GetProperty("id").GetString(),
                    adminQuery.GetProperty("data").GetProperty("getCustomers")[0]
                        .GetProperty("id").GetString());
            }
            finally { await CleanupCustomerAsync(CreateApiClient(adminToken), customerId); }
        }
    }
}