using Customer_Mangment.CQRS.Identity.Dto;
using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces.AppMediator;

namespace Customer_Mangment.CQRS.Identity.Queries.RefreshTokens;

public record RefreshTokenQuery(string RefreshToken, string ExpiredAccessToken) : IAppRequest<Result<TokenResponse>>;