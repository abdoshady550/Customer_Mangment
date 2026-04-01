using Customer_Mangment.Model.Results;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Customer_Mangment.Model.Entities
{
    public sealed class RefreshToken
    {
        [BsonId]
        [BsonGuidRepresentation(GuidRepresentation.Standard)]
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

        public static Result<RefreshToken> Create(Guid id, string? token, string? userId, DateTimeOffset expiresOnUtc, RefreshTokenErrors tokenErrors)
        {
            if (id == Guid.Empty)
            {
                return tokenErrors.IdRequired;

            }

            if (string.IsNullOrWhiteSpace(token))
            {
                return tokenErrors.TokenRequired;
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                return tokenErrors.UserIdRequired;
            }

            if (expiresOnUtc <= DateTimeOffset.UtcNow)
            {
                return tokenErrors.ExpiryInvalid;
            }

            return new RefreshToken(id, token, userId, expiresOnUtc);
        }
    }
}
