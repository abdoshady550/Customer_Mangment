using Customer_Mangment.CQRS.Webhooks.DTOs;
using Customer_Mangment.Repository.Interfaces.AppMediator;

namespace Customer_Mangment.CQRS.Webhooks.Commands;

public sealed record CreateWebhookCommand(string UserId, string Url, string[] Events) : IAppRequest<Model.Results.Result<WebhookDto>>;