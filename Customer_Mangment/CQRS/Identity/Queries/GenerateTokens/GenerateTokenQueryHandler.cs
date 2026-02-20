using Customer_Mangment.CQRS.Identity.Dto;
using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces;
using MediatR;

namespace Customer_Mangment.CQRS.Identity.Queries.GenerateTokens;

public class GenerateTokenQueryHandler(ILogger<GenerateTokenQueryHandler> logger, IIdentityService identityService, ITokenProvider tokenProvider)
    : IRequestHandler<GenerateTokenQuery, Result<TokenResponse>>
{
    private readonly ILogger<GenerateTokenQueryHandler> _logger = logger;
    private readonly IIdentityService _identityService = identityService;
    private readonly ITokenProvider _tokenProvider = tokenProvider;

    public async Task<Result<TokenResponse>> Handle(GenerateTokenQuery query, CancellationToken ct)
    {
        var userResponse = await _identityService.AuthenticateAsync(query.Email, query.Password);

        if (userResponse.IsError)
        {
            return userResponse.Errors;
        }

        var generateTokenResult = await _tokenProvider.GenerateJwtTokenAsync(userResponse.Value, ct);

        if (generateTokenResult.IsError)
        {
            _logger.LogError("Generate token error occurred: {ErrorDescription}", generateTokenResult.TopError.Description);

            return generateTokenResult.Errors;
        }

        return generateTokenResult.Value;
    }
}