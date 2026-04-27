using Customer_Mangment.CQRS.Identity.Dto;
using Customer_Mangment.Repository.Interfaces;
using Customer_Mangment.Repository.Interfaces.AppMediator;
using Customer_Mangment.SharedResources;
using Customer_Mangment.SharedResources.Keys;
using Microsoft.Extensions.Localization;

namespace Customer_Mangment.CQRS.Identity.Commands.GenerateTokens;

public class GenerateTokenComandHandler(ILogger<GenerateTokenComandHandler> logger,
                                        IIdentityService identityService,
                                        IIdentityServerTokenService identityServerToken,
                                        IStringLocalizer<SharedResource> localizer)
    : IAppRequestHandler<GenerateTokenComand, Model.Results.Result<TokenServerResponse>>
{
    private readonly ILogger<GenerateTokenComandHandler> _logger = logger;
    private readonly IIdentityService _identityService = identityService;
    private readonly IIdentityServerTokenService _identityServerToken = identityServerToken;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Model.Results.Result<TokenServerResponse>> Handle(GenerateTokenComand query, CancellationToken ct)
    {
        var userResponse = await _identityService.AuthenticateAsync(query.Email, query.Password);

        if (userResponse.IsError)
        {
            _logger.LogWarning("gRPC : Authentication failed for user {Email}.", query.Email);
            return userResponse.Errors;
        }

        var result = await _identityServerToken.RequestPasswordTokenAsync(query.Email, query.Password, query.TenantId, ct);

        if (result is null || string.IsNullOrEmpty(result.AccessToken))
        {
            _logger.LogWarning("Failed to generate token for user {Email}.", query.Email);
            return LocalizedError.Unauthorized(_localizer, "LoginFailed", ResourceKeys.Auth.Unauthorized);
        }
        return result;

    }
}