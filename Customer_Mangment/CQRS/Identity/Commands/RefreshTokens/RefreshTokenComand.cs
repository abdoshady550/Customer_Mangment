using Customer_Mangment.CQRS.Identity.Dto;
using Customer_Mangment.Repository.Interfaces.AppMediator;

namespace Customer_Mangment.CQRS.Identity.Queries.RefreshTokens;

public record RefreshTokenComand(string RefreshToken, string ExpiredAccessToken) : IAppRequest<Model.Results.Result<TokenServerResponse>>;