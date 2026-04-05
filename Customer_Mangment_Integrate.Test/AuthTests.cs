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
            var result = await CreateApiClient().GenerateTokenAsync(new GenerateTokenQuery
            {
                Email = AdminEmail,
                Password = AdminPassword
            });

            Assert.NotNull(result);
            Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
            Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
            Assert.True(result.ExpiresOnUtc > DateTimeOffset.UtcNow);
        }

        [Fact]
        public async Task GenerateToken_UserValidCredentials_ReturnsTokenPair()
        {
            var result = await CreateApiClient().GenerateTokenAsync(new GenerateTokenQuery
            {
                Email = UserEmail,
                Password = UserPassword
            });

            Assert.NotNull(result);
            Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
            Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
            Assert.True(result.ExpiresOnUtc > DateTimeOffset.UtcNow);
        }

        [Fact]
        public async Task GenerateToken_WrongPassword_ThrowsBadRequestOrNotFound()
        {
            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => CreateApiClient().GenerateTokenAsync(new GenerateTokenQuery
                {
                    Email = AdminEmail,
                    Password = "WrongPassword!"
                }));

            Assert.True(ex.StatusCode == 400 || ex.StatusCode == 404 || ex.StatusCode == 409,
                $"Expected 400 or 404 or 409 but got {ex.StatusCode}");
        }

        [Fact]
        public async Task GenerateToken_UnknownEmail_ThrowsNotFound()
        {
            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => CreateApiClient().GenerateTokenAsync(new GenerateTokenQuery
                {
                    Email = "nobody@nowhere.com",
                    Password = "Test@123"
                }));

            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task GenerateToken_EmptyEmail_ThrowsError()
        {
            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => CreateApiClient().GenerateTokenAsync(new GenerateTokenQuery
                {
                    Email = "",
                    Password = AdminPassword
                }));

            Assert.True(ex.StatusCode == 400 || ex.StatusCode == 404);
        }

        [Fact]
        public async Task GenerateToken_EmptyPassword_ThrowsError()
        {
            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => CreateApiClient().GenerateTokenAsync(new GenerateTokenQuery
                {
                    Email = AdminEmail,
                    Password = ""
                }));

            Assert.True(ex.StatusCode == 400 || ex.StatusCode == 404);
        }

        [Fact]
        public async Task GenerateToken_AdminAndUser_ReturnDifferentTokens()
        {
            var adminToken = await CreateApiClient().GenerateTokenAsync(new GenerateTokenQuery
            {
                Email = AdminEmail,
                Password = AdminPassword
            });
            var userToken = await CreateApiClient().GenerateTokenAsync(new GenerateTokenQuery
            {
                Email = UserEmail,
                Password = UserPassword
            });

            Assert.NotEqual(adminToken.AccessToken, userToken.AccessToken);
        }

        [Fact]
        public async Task RefreshToken_WithValidToken_ReturnsNewTokenPair()
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

            Assert.NotNull(refreshed);
            Assert.False(string.IsNullOrWhiteSpace(refreshed.AccessToken));
            Assert.False(string.IsNullOrWhiteSpace(refreshed.RefreshToken));
            Assert.True(refreshed.ExpiresOnUtc > DateTimeOffset.UtcNow);
        }

        [Fact]
        public async Task RefreshToken_NewTokensDifferentFromOriginal()
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

            Assert.NotEqual(initial.AccessToken, refreshed.AccessToken);
            Assert.NotEqual(initial.RefreshToken, refreshed.RefreshToken);
        }

        [Fact]
        public async Task RefreshToken_InvalidTokens_ThrowsError()
        {
            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => CreateApiClient().RefreshTokenAsync(new RefreshTokenQuery
                {
                    RefreshToken = "invalid-refresh-token",
                    ExpiredAccessToken = "invalid-access-token"
                }));

            Assert.True(ex.StatusCode == 400 || ex.StatusCode == 404 || ex.StatusCode == 409,
                $"Expected 400 or 404 but got {ex.StatusCode}");
        }

        [Fact]
        public async Task RefreshToken_EmptyRefreshToken_ThrowsError()
        {
            var client = CreateApiClient();
            var initial = await client.GenerateTokenAsync(new GenerateTokenQuery
            {
                Email = AdminEmail,
                Password = AdminPassword
            });

            var ex = await Assert.ThrowsAnyAsync<ApiException>(
                () => client.RefreshTokenAsync(new RefreshTokenQuery
                {
                    RefreshToken = "",
                    ExpiredAccessToken = initial.AccessToken
                }));

            Assert.True(ex.StatusCode == 400 || ex.StatusCode == 404 || ex.StatusCode == 409,
                $"Expected 400 or 404 but got {ex.StatusCode}");
        }

        [Fact]
        public async Task RefreshToken_UserToken_ReturnsNewPair()
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

            Assert.NotNull(refreshed);
            Assert.False(string.IsNullOrWhiteSpace(refreshed.AccessToken));
        }

        [Fact]
        public async Task RefreshToken_NewTokenCanAccessProtectedEndpoint()
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

            var authed = CreateApiClient(refreshed.AccessToken);
            var customers = await authed.Get2Async(null);
            Assert.NotNull(customers);
        }
    }
}
