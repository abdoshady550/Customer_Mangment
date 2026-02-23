using Customer_Mangment.CQRS.Identity.Dto;
using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces.AppMediator;

namespace Customer_Mangment.CQRS.Identity.Queries.GenerateTokens;

public record GenerateTokenQuery(
    string Email,
    string Password) : IAppRequest<Result<TokenResponse>>;