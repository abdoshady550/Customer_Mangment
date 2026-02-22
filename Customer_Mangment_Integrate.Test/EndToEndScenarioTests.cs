using Customer_Mangment;
using Customer_Mangment_Integrate.Test.Common;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Customer_Mangment_Integrate.Test
{
    public class EndToEndScenarioTests : TestBase
    {
        public EndToEndScenarioTests(WebApplicationFactory<IAssmblyMarker> factory) : base(factory) { }

        [Fact]
        public async Task FullCustomerLifecycle_CreateUpdateDelete_WorksCorrectly()
        {
            var token = await GetTokenAsync();
            var client = CreateApiClient(token);

            // 1. Create
            var created = await CreateTestCustomerAsync(client, "Lifecycle User", "01000000001");
            Assert.NotEqual(Guid.Empty, created.Id);

            // 2. Read
            var fetched = (await client.CustomerAllAsync(created.Id)).First();
            Assert.Equal("Lifecycle User", fetched.Name);

            // 3. Update
            await client.CustomerPUTAsync(new UpdateCustomerReq
            {
                CustomerId = created.Id,
                Name = "Lifecycle User Updated",
                Mobile = "01000000002"
            });

            var afterUpdate = (await client.CustomerAllAsync(created.Id)).First();
            Assert.Equal("Lifecycle User Updated", afterUpdate.Name);

            // 4. Add Address
            var address = await client.CustomerAddressPOSTAsync(new AddAddressReq
            {
                AddrssId = created.Id,
                Type = 1,
                Value = "Test Address"
            });
            Assert.NotEqual(Guid.Empty, address.Id);

            // 5. Update Address
            await client.UpdateAsync(new UpdateAddressReq
            {
                AddressId = address.Id,
                Type = 2,
                Value = "Updated Test Address"
            });

            // 6. Delete Address
            await client.CustomerAddressDELETEAsync(address.Id);

            // 7. Check History
            var history = await client.HistoryAsync(created.Id);
            Assert.True(history.Count >= 2);

            // 8. Delete Customer
            await client.CustomerDELETEAsync(created.Id);

            // 9. Verify deleted
            var ex = await Assert.ThrowsAsync<ApiException<ProblemDetails>>(
                () => client.CustomerAllAsync(created.Id));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task TokenRefresh_ThenCallProtectedEndpoint_Succeeds()
        {
            var anonClient = CreateApiClient();

            // 1. Get initial token
            var initial = await anonClient.GenerateTokenAsync(new GenerateTokenQuery
            {
                Email = "admin@test.com",
                Password = "Admin@123"
            });

            // 2. Refresh the token
            var refreshed = await anonClient.RefreshTokenAsync(new RefreshTokenQuery
            {
                RefreshToken = initial.RefreshToken,
                ExpiredAccessToken = initial.AccessToken
            });

            // 3. Use refreshed token to call a protected endpoint
            var authClient = CreateApiClient(refreshed.AccessToken);
            var customers = await authClient.CustomerAllAsync(null);

            Assert.NotNull(customers);
        }
    }

}
