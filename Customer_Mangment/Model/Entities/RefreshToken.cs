using Customer_Mangment.Model.Results;

namespace Customer_Mangment.Model.Entities
{
    public sealed class RefreshToken
    {
        public Guid Id { get; private set; }

        public string? Token { get; private set; }
        public string? UserId { get; private set; }
        public DateTimeOffset ExpiresOnUtc { get; private set; }

        private RefreshToken()
        { }

        private RefreshToken(Guid id, string? token, string? userId, DateTimeOffset expiresOnUtc)
        {
            Id = id;
            Token = token;
            UserId = userId;
            ExpiresOnUtc = expiresOnUtc;
        }

        public static Result<RefreshToken> Create(Guid id, string? token, string? userId, DateTimeOffset expiresOnUtc)
        {
            if (id == Guid.Empty)
            {
                return RefreshTokenErrors.IdRequired;
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                return RefreshTokenErrors.TokenRequired;
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                return RefreshTokenErrors.UserIdRequired;
            }

            if (expiresOnUtc <= DateTimeOffset.UtcNow)
            {
                return RefreshTokenErrors.ExpiryInvalid;
            }

            return new RefreshToken(id, token, userId, expiresOnUtc);
        }
    }
}
