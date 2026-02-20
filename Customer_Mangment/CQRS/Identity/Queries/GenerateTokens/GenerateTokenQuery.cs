using Customer_Mangment.CQRS.Identity.Dto;
using Customer_Mangment.Model.Results;
using MediatR;

namespace Customer_Mangment.CQRS.Identity.Queries.GenerateTokens;

public record GenerateTokenQuery(
    string Email,
    string Password) : IRequest<Result<TokenResponse>>;