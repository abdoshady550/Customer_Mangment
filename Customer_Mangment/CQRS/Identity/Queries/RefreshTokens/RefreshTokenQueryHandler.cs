using Customer_Mangment.CQRS.Identity.Dto;
using Customer_Mangment.Model;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces;
using Customer_Mangment.Repository.Interfaces.AppMediator;
using System.Security.Claims;

namespace Customer_Mangment.CQRS.Identity.Queries.RefreshTokens;

public class RefreshTokenQueryHandler(ILogger<RefreshTokenQueryHandler> logger,
                                      IIdentityService identityService,
                                      IGenericRepo<RefreshToken> repo,
                                      ITokenProvider tokenProvider)
    : IAppRequestHandler<RefreshTokenQuery, Result<TokenResponse>>
{
    private readonly ILogger<RefreshTokenQueryHandler> _logger = logger;
    private readonly IIdentityService _identityService = identityService;
    private readonly IGenericRepo<RefreshToken> _repo = repo;
    private readonly ITokenProvider _tokenProvider = tokenProvider;

    public async Task<Result<TokenResponse>> Handle(RefreshTokenQuery request, CancellationToken ct)
    {
        var principal = _tokenProvider.GetPrincipalFromExpiredToken(request.ExpiredAccessToken);

        if (principal is null)
        {
            _logger.LogError("Expired access token is not valid");

            return RefreshTokenErrors.ExpiredAccessTokenInvalid;
        }

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId is null)
        {
            _logger.LogError("Invalid userId claim");

            return RefreshTokenErrors.UserIdClaimInvalid;
        }

        var getUserResult = await _identityService.GetUserByIdAsync(userId);

        if (getUserResult.IsError)
        {
            _logger.LogError("Get user by id error occurred: {ErrorDescription}", getUserResult.TopError.Description);
            return getUserResult.Errors;
        }

        var refreshToken = await _repo.FirstOrDefaultAsync(r => r.Token == request.RefreshToken && r.UserId == userId, ct);

        if (refreshToken is null || refreshToken.ExpiresOnUtc < DateTime.UtcNow)
        {
            _logger.LogError("Refresh token has expired");

            return RefreshTokenErrors.RefreshTokenExpired;
        }

        var generateTokenResult = await _tokenProvider.GenerateJwtTokenAsync(getUserResult.Value, ct);

        if (generateTokenResult.IsError)
        {
            _logger.LogError("Generate token error occurred: {ErrorDescription}", generateTokenResult.TopError.Description);

            return generateTokenResult.Errors;
        }

        return generateTokenResult.Value;
    }
}