
using Customer_Mangment.Model.Results;

namespace Customer_Mangment.Model;

public static class RefreshTokenErrors
{
    public static readonly Error IdRequired =
        Error.Validation("RefreshToken_Id_Required", "Refresh token ID is required.");

    public static readonly Error TokenRequired =
        Error.Validation("RefreshToken_Token_Required", "Token value is required.");

    public static readonly Error UserIdRequired =
        Error.Validation("RefreshToken_UserId_Required", "User ID is required.");

    public static readonly Error ExpiryInvalid =
        Error.Validation("RefreshToken_Expiry_Invalid", "Expiry must be in the future.");
    public static Error InvalidRefreshToken =>
        Error.Validation("RefreshToken.Expiry.Invalid", "Expiry must be in the future.");

    public static readonly Error ExpiredAccessTokenInvalid = Error.Conflict(
         code: "Auth.ExpiredAccessToken.Invalid",
         description: "Expired access token is not valid.");

    public static readonly Error UserIdClaimInvalid = Error.Conflict(
        code: "Auth.UserIdClaim.Invalid",
        description: "Invalid userId claim.");

    public static readonly Error RefreshTokenExpired = Error.Conflict(
        code: "Auth.RefreshToken.Expired",
        description: "Refresh token is invalid or has expired.");

    public static readonly Error UserNotFound = Error.NotFound(
        code: "Auth.User.NotFound",
        description: "User not found.");

    public static readonly Error TokenGenerationFailed = Error.Failure(
        code: "Auth.TokenGeneration.Failed",
        description: "Failed to generate new JWT token.");
}