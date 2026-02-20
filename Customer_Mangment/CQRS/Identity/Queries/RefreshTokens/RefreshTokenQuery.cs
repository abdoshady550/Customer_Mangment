using Customer_Mangment.CQRS.Identity.Dto;
using Customer_Mangment.Model.Results;
using MediatR;

namespace Customer_Mangment.CQRS.Identity.Queries.RefreshTokens;

public record RefreshTokenQuery(string RefreshToken, string ExpiredAccessToken) : IRequest<Result<TokenResponse>>;