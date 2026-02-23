using Customer_Mangment;
using Customer_Mangment_Integrate.Test.Common;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Customer_Mangment_Integrate.Test
{
    public class AuthTests : TestBase
    {
        public AuthTests(WebApplicationFactory<IAssmblyMarker> factory) : base(factory) { }

        [Fact]
        public async Task GenerateToken_AdminValidCredentials_ReturnsTokenPair()
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
        public async Task GenerateToken_UserValidCredentials_ReturnsTokenPair()
        {
            var client = CreateApiClient();

            var result = await client.GenerateTokenAsync(new GenerateTokenQuery
            {
                Email = "user@test.com",
                Password = "User@123"
            });

            Assert.NotNull(result);
            Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
            Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
        }

        [Fact]
        public async Task GenerateToken_WrongPassword_ThrowsError()
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
        public async Task GenerateToken_UnknownEmail_ThrowsNotFound()
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
        public async Task RefreshToken_WithValidToken_ReturnsNewTokenPair()
        {
            var client = CreateApiClient();

            var initial = await client.GenerateTokenAsync(new GenerateTokenQuery
            {
                Email = "admin@test.com",
                Password = "Admin@123"
            });


            var refreshed = await client.RefreshTokenAsync(new RefreshTokenQuery
            {
                RefreshToken = initial.RefreshToken,
                ExpiredAccessToken = initial.AccessToken
            });

            Assert.NotNull(refreshed);
            Assert.False(string.IsNullOrWhiteSpace(refreshed.AccessToken));
            Assert.False(string.IsNullOrWhiteSpace(refreshed.RefreshToken));
            Assert.True(refreshed.ExpiresOnUtc > DateTimeOffset.UtcNow);
        }

        [Fact]
        public async Task RefreshToken_InvalidTokens_ThrowsError()
        {
            var client = CreateApiClient();

            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => client.RefreshTokenAsync(new RefreshTokenQuery
                {
                    RefreshToken = "invalid-refresh-token",
                    ExpiredAccessToken = "invalid-access-token"
                }));

            Assert.True(ex.StatusCode == 500,
                $"Expected 500 but got {ex.StatusCode}");
        }
        [Fact]
        public async Task RefreshToken_NewTokenIsDifferentFromOldToken()
        {
            var client = CreateApiClient();

            var initial = await client.GenerateTokenAsync(new GenerateTokenQuery
            {
                Email = "admin@test.com",
                Password = "Admin@123"
            });

            var refreshed = await client.RefreshTokenAsync(new RefreshTokenQuery
            {
                RefreshToken = initial.RefreshToken,
                ExpiredAccessToken = initial.AccessToken
            });


            Assert.NotEqual(initial.AccessToken, refreshed.AccessToken);
            Assert.NotEqual(initial.RefreshToken, refreshed.RefreshToken);
        }
    }

}
