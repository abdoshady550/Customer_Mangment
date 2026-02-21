using Customer_Mangment.Model;
using Customer_Mangment.Model.Entities;

namespace Customer_Mangment.Test.Domain
{
    public class RefreshTokenTest
    {
        [Fact]
        public void Create_ShouldReturnError_WhenIdIsEmpty()
        {
            var result = RefreshToken.Create(
                Guid.Empty,
                "token123",
                "user1",
                DateTimeOffset.UtcNow.AddDays(1));

            Assert.False(result.IsSuccess);
            Assert.Equal(RefreshTokenErrors.IdRequired.Code, result.TopError.Code);
        }

        [Fact]
        public void Create_ShouldReturnError_WhenTokenIsNullOrWhiteSpace()
        {
            string firstToken = null;
            string secondToken = "";

            var firstResult = RefreshToken.Create(
                Guid.NewGuid(),
                firstToken,
                "user1",
                DateTimeOffset.UtcNow.AddDays(1));

            var secondResult = RefreshToken.Create(
                Guid.NewGuid(),
                secondToken,
                "user1",
                DateTimeOffset.UtcNow.AddDays(1));

            Assert.False(firstResult.IsSuccess);
            Assert.False(secondResult.IsSuccess);
            Assert.Equal(RefreshTokenErrors.TokenRequired.Code, firstResult.TopError.Code);
        }
        [Fact]
        public void Create_ShouldReturnError_WhenUserIdIsNullOrWhiteSpace()
        {
            string firstUserId = null;
            string secondUserId = "";

            var firstResult = RefreshToken.Create(
                Guid.NewGuid(),
                "token123",
                firstUserId,
                DateTimeOffset.UtcNow.AddDays(1));

            var secondResult = RefreshToken.Create(
                Guid.NewGuid(),
                "token123",
                secondUserId,
                DateTimeOffset.UtcNow.AddDays(1));

            Assert.False(firstResult.IsSuccess);
            Assert.False(secondResult.IsSuccess);
            Assert.Equal(RefreshTokenErrors.UserIdRequired.Code, firstResult.TopError.Code);
        }
        [Fact]
        public void Create_ShouldReturnError_WhenExpiryIsInPast()
        {
            var result = RefreshToken.Create(
                Guid.NewGuid(),
                "token123",
                "user1",
                DateTimeOffset.UtcNow.AddMinutes(-1));

            Assert.False(result.IsSuccess);
            Assert.Equal(RefreshTokenErrors.ExpiryInvalid.Code, result.TopError.Code);
        }
        [Fact]
        public void Create_ShouldReturnSuccess_WhenDataIsValid()
        {
            var result = RefreshToken.Create(
                Guid.NewGuid(),
                "token123",
                "user1",
                DateTimeOffset.UtcNow.AddDays(1));

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
        }
    }
}
