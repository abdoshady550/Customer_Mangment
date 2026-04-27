using Customer_Mangment.CQRS.Identity.Dto;
using Customer_Mangment.Model;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.Repository.Interfaces;
using Customer_Mangment.Repository.Interfaces.AppMediator;

namespace Customer_Mangment.CQRS.Identity.Queries.RefreshTokens;

public class RefreshTokenComandHandler(ILogger<RefreshTokenQueryHandler> logger,
                                      IIdentityService identityService,
                                      IGenericRepo<RefreshToken> repo,
                                      RefreshTokenErrors tokenErrors,
                                      ITokenProvider tokenProvider,
                                      IIdentityServerTokenService identityServerToken)
    : IAppRequestHandler<RefreshTokenComand, Model.Results.Result<TokenServerResponse>>
{
    private readonly ILogger<RefreshTokenQueryHandler> _logger = logger;
    private readonly IIdentityService _identityService = identityService;
    private readonly IGenericRepo<RefreshToken> _repo = repo;
    private readonly RefreshTokenErrors _tokenErrors = tokenErrors;
    private readonly ITokenProvider _tokenProvider = tokenProvider;
    private readonly IIdentityServerTokenService _identityServerToken = identityServerToken;


    public async Task<Model.Results.Result<TokenServerResponse>> Handle(RefreshTokenComand request, CancellationToken ct)
    {
        var result = await _identityServerToken.RefreshTokenAsync(request.RefreshToken, null, ct);
        if (result is null)
        {
            _logger.LogError("Token refresh failed for refresh token {RefreshToken}.", request.RefreshToken);
            return _tokenErrors.TokenGenerationFailed;
        }
        return result;
    }
}