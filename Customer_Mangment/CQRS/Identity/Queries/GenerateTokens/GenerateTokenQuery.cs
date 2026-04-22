using Customer_Mangment.CQRS.Identity.Dto;
using Customer_Mangment.Repository.Interfaces.AppMediator;

namespace Customer_Mangment.CQRS.Identity.Queries.GenerateTokens;

public record GenerateTokenQuery(
    string Email,
    string Password) : IAppRequest<Model.Results.Result<TokenResponse>>;