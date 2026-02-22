using Customer_Mangment;
using Customer_Mangment_Integrate.Test.Common;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Customer_Mangment_Integrate.Test
{
    public class AuthTests : TestBase
    {
        public AuthTests(WebApplicationFactory<IAssmblyMarker> factory) : base(factory) { }
        [Fact]
        public async Task GenerateToken_ValidCredentials_ReturnsAccessAndRefreshToken()
        {
            var client = CreateApiClient();


            var result = await client.GenerateTokenAsync(new GenerateTokenQuery
            {
                Email = "admin@test.com",
                Password = "Admin@123"
            });

            Assert.NotNull(result);
            Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
            Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
            Assert.True(result.ExpiresOnUtc > DateTimeOffset.UtcNow);
        }

        [Fact]
        public async Task GenerateToken_WrongPassword_ThrowsBadRequest()
        {
            var client = CreateApiClient();

            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => client.GenerateTokenAsync(new GenerateTokenQuery
                {
                    Email = "admin@test.com",
                    Password = "WrongPassword!"
                }));

            Assert.Equal(409, ex.StatusCode);
        }

        [Fact]
        public async Task GenerateToken_UnknownEmail_ThrowsBadRequest()
        {
            var client = CreateApiClient();

            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => client.GenerateTokenAsync(new GenerateTokenQuery
                {
                    Email = "nobody@nowhere.com",
                    Password = "Test@123"
                }));

            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task RefreshToken_ValidRefreshToken_ReturnsNewTokenPair()
        {
            // Arrange: get initial tokens
            var client = CreateApiClient();
            var initial = await client.GenerateTokenAsync(new GenerateTokenQuery
            {
                Email = "admin@test.com",
                Password = "Admin@123"
            });

            // Act
            var refreshed = await client.RefreshTokenAsync(new RefreshTokenQuery
            {
                RefreshToken = initial.RefreshToken,
                ExpiredAccessToken = initial.AccessToken
            });

            // Assert
            Assert.NotNull(refreshed);
            Assert.False(string.IsNullOrWhiteSpace(refreshed.AccessToken));
            Assert.False(string.IsNullOrWhiteSpace(refreshed.RefreshToken));
        }

        [Fact]
        public async Task RefreshToken_InvalidRefreshToken_ThrowsBadRequest()
        {
            var client = CreateApiClient();

            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => client.RefreshTokenAsync(new RefreshTokenQuery
                {
                    RefreshToken = "totally-invalid-token",
                    ExpiredAccessToken = "totally-invalid-access-token"
                }));

            Assert.Equal(500, ex.StatusCode);
        }
    }

}
