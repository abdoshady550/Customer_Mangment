using Customer_Mangment.Model.Results;
using Customer_Mangment.SharedResources;
using Customer_Mangment.SharedResources.Keys;
using Microsoft.Extensions.Localization;

namespace Customer_Mangment.Model;

public sealed class RefreshTokenErrors(IStringLocalizer<SharedResource> l)
{
    public Error IdRequired =>
        LocalizedError.Validation(l, "RefreshToken_Id_Required", ResourceKeys.Token.IdRequired);

    public Error TokenRequired =>
        LocalizedError.Validation(l, "RefreshToken_Token_Required", ResourceKeys.Token.TokenRequired);

    public Error UserIdRequired =>
        LocalizedError.Validation(l, "RefreshToken_UserId_Required", ResourceKeys.Token.UserIdRequired);

    public Error ExpiryInvalid =>
        LocalizedError.Validation(l, "RefreshToken_Expiry_Invalid", ResourceKeys.Token.ExpiryInvalid);

    public Error InvalidRefreshToken =>
        LocalizedError.Validation(l, "RefreshToken.Expiry.Invalid", ResourceKeys.Token.ExpiryInvalid);

    public Error ExpiredAccessTokenInvalid =>
        LocalizedError.Conflict(l, "Auth.ExpiredAccessToken.Invalid", ResourceKeys.Auth.TokenExpired);

    public Error UserIdClaimInvalid =>
        LocalizedError.Conflict(l, "Auth.UserIdClaim.Invalid", ResourceKeys.Auth.UserIdClaimInvalid);

    public Error RefreshTokenExpired =>
        LocalizedError.Conflict(l, "Auth.RefreshToken.Expired", ResourceKeys.Auth.RefreshTokenExpired);

    public Error UserNotFound =>
        LocalizedError.NotFound(l, "Auth.User.NotFound", ResourceKeys.User.NotFound);

    public Error TokenGenerationFailed =>
        LocalizedError.Failure(l, "Auth.TokenGeneration.Failed", ResourceKeys.Token.GenerationFailed);
}