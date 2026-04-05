using Customer_Mangment.Model;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.SharedResources;
using Microsoft.Extensions.Localization;
using NSubstitute;

namespace Customer_Mangment.Test.Domain
{
    public class RefreshTokenTest
    {
        private static RefreshTokenErrors CreateErrors()
        {
            var localizer = Substitute.For<IStringLocalizer<SharedResource>>();
            localizer[Arg.Any<string>()].Returns(ci =>
                new LocalizedString(ci.Arg<string>(), ci.Arg<string>()));
            return new RefreshTokenErrors(localizer);
        }

        [Fact]
        public void Create_ShouldReturnError_WhenIdIsEmpty()
        {
            var errors = CreateErrors();

            var result = RefreshToken.Create(
                Guid.Empty,
                "token123",
                "user1",
                DateTimeOffset.UtcNow.AddDays(1),
                errors);

            Assert.False(result.IsSuccess);
            Assert.Equal(errors.IdRequired.Code, result.TopError.Code);
        }

        [Fact]
        public void Create_ShouldReturnError_WhenTokenIsNull()
        {
            var errors = CreateErrors();

            var result = RefreshToken.Create(
                Guid.NewGuid(),
                null,
                "user1",
                DateTimeOffset.UtcNow.AddDays(1),
                errors);

            Assert.False(result.IsSuccess);
            Assert.Equal(errors.TokenRequired.Code, result.TopError.Code);
        }

        [Fact]
        public void Create_ShouldReturnError_WhenTokenIsEmpty()
        {
            var errors = CreateErrors();

            var result = RefreshToken.Create(
                Guid.NewGuid(),
                "",
                "user1",
                DateTimeOffset.UtcNow.AddDays(1),
                errors);

            Assert.False(result.IsSuccess);
            Assert.Equal(errors.TokenRequired.Code, result.TopError.Code);
        }

        [Fact]
        public void Create_ShouldReturnError_WhenTokenIsWhiteSpace()
        {
            var errors = CreateErrors();

            var result = RefreshToken.Create(
                Guid.NewGuid(),
                "   ",
                "user1",
                DateTimeOffset.UtcNow.AddDays(1),
                errors);

            Assert.False(result.IsSuccess);
            Assert.Equal(errors.TokenRequired.Code, result.TopError.Code);
        }

        [Fact]
        public void Create_ShouldReturnError_WhenUserIdIsNull()
        {
            var errors = CreateErrors();

            var result = RefreshToken.Create(
                Guid.NewGuid(),
                "token123",
                null,
                DateTimeOffset.UtcNow.AddDays(1),
                errors);

            Assert.False(result.IsSuccess);
            Assert.Equal(errors.UserIdRequired.Code, result.TopError.Code);
        }

        [Fact]
        public void Create_ShouldReturnError_WhenUserIdIsEmpty()
        {
            var errors = CreateErrors();

            var result = RefreshToken.Create(
                Guid.NewGuid(),
                "token123",
                "",
                DateTimeOffset.UtcNow.AddDays(1),
                errors);

            Assert.False(result.IsSuccess);
            Assert.Equal(errors.UserIdRequired.Code, result.TopError.Code);
        }

        [Fact]
        public void Create_ShouldReturnError_WhenExpiryIsInPast()
        {
            var errors = CreateErrors();

            var result = RefreshToken.Create(
                Guid.NewGuid(),
                "token123",
                "user1",
                DateTimeOffset.UtcNow.AddMinutes(-1),
                errors);

            Assert.False(result.IsSuccess);
            Assert.Equal(errors.ExpiryInvalid.Code, result.TopError.Code);
        }

        [Fact]
        public void Create_ShouldReturnError_WhenExpiryIsExactlyNow()
        {
            var errors = CreateErrors();

            var result = RefreshToken.Create(
                Guid.NewGuid(),
                "token123",
                "user1",
                DateTimeOffset.UtcNow,
                errors);

            Assert.False(result.IsSuccess);
            Assert.Equal(errors.ExpiryInvalid.Code, result.TopError.Code);
        }

        [Fact]
        public void Create_ShouldSucceed_WhenAllDataIsValid()
        {
            var errors = CreateErrors();
            var id = Guid.NewGuid();
            var expiry = DateTimeOffset.UtcNow.AddDays(1);

            var result = RefreshToken.Create(id, "token123", "user1", expiry, errors);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
        }

        [Fact]
        public void Create_ShouldPreserveValues_WhenSuccessful()
        {
            var errors = CreateErrors();
            var id = Guid.NewGuid();
            var expiry = DateTimeOffset.UtcNow.AddDays(7);

            var result = RefreshToken.Create(id, "mytoken", "user42", expiry, errors);

            Assert.True(result.IsSuccess);
            Assert.Equal(id, result.Value.Id);
            Assert.Equal("mytoken", result.Value.Token);
            Assert.Equal("user42", result.Value.UserId);
            Assert.Equal(expiry, result.Value.ExpiresOnUtc);
        }

        [Fact]
        public void Create_ShouldValidateIdBeforeToken()
        {
            var errors = CreateErrors();

            // Both id empty and token null — id check should fire first
            var result = RefreshToken.Create(
                Guid.Empty,
                null,
                "user1",
                DateTimeOffset.UtcNow.AddDays(1),
                errors);

            Assert.False(result.IsSuccess);
            Assert.Equal(errors.IdRequired.Code, result.TopError.Code);
        }
    }
}
