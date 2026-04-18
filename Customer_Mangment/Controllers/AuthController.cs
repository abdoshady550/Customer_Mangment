using Asp.Versioning;
using Customer_Mangment.CQRS.Identity.Dto;
using Customer_Mangment.CQRS.Identity.Queries.GenerateTokens;
using Customer_Mangment.CQRS.Identity.Queries.RefreshTokens;
using Customer_Mangment.Repository.Interfaces;
using Customer_Mangment.Repository.Interfaces.AppMediator;
using Customer_Mangment.SharedResources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using static Customer_Mangment.SharedResources.Keys.ResourceKeys;

namespace Customer_Mangment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]

    public sealed class AuthController(IDispatcher sender, IStringLocalizer<SharedResource> localizer, IIdentityServerTokenService identityServerToken) : ApiController(localizer)
    {
        private readonly IDispatcher _sender = sender;
        private readonly IIdentityServerTokenService _identityServerToken = identityServerToken;

        [HttpPost("token/generate")]
        [ProducesResponseType(typeof(TokenServerResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointSummary("Generates an access and refresh token for a valid user.")]
        [EndpointDescription("Authenticates a user using provided credentials and returns a JWT token pair.")]
        [EndpointName("GenerateToken")]
        public async Task<IActionResult> GenerateToken([FromBody] GenerateTokenQuery request, CancellationToken ct)
        {
            var tenantId = Request.Headers["X-Tenant-Id"].FirstOrDefault();

            var result = await _identityServerToken.RequestPasswordTokenAsync(request.Email, request.Password, tenantId, ct);
            if (string.IsNullOrEmpty(result.AccessToken))
                return Problem(statusCode: StatusCodes.Status500InternalServerError, title: localizer[Auth.Unauthorized]);

            var response = new TokenServerResponse
            {
                AccessToken = result.AccessToken,
                RefreshToken = result.RefreshToken,
                ExpiresIn = result.ExpiresIn,
                TokenType = result.TokenType
            };
            return Ok(response);
        }

        [HttpPost("token/refresh-token")]
        [ProducesResponseType(typeof(TokenServerResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointSummary("Refreshes access token using a valid refresh token.")]
        [EndpointDescription("Exchanges an expired access token and a valid refresh token for a new token pair.")]
        [EndpointName("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenQuery request, CancellationToken ct)
        {
            var result = await _identityServerToken.RefreshTokenAsync(request.RefreshToken, null, ct);
            if (result is null)
            {
                return Problem(statusCode: StatusCodes.Status500InternalServerError, title: localizer["TokenRefreshFailed"]);
            }
            else
            {
                var response = new TokenServerResponse
                {
                    AccessToken = result.AccessToken,
                    RefreshToken = result.RefreshToken,
                    ExpiresIn = result.ExpiresIn,
                    TokenType = result.TokenType
                };
                return Ok(response);

            }
        }

    }
}
