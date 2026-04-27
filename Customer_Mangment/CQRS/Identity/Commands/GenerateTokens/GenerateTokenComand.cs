using Customer_Mangment.CQRS.Identity.Dto;
using Customer_Mangment.Repository.Interfaces.AppMediator;

namespace Customer_Mangment.CQRS.Identity.Commands.GenerateTokens;

public record GenerateTokenComand(
    string Email,
    string Password,
    string? TenantId) : IAppRequest<Model.Results.Result<TokenServerResponse>>;