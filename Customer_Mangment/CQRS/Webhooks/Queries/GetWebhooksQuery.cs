using Customer_Mangment.CQRS.Webhooks.DTOs;
using Customer_Mangment.Repository.Interfaces.AppMediator;

namespace Customer_Mangment.CQRS.Webhooks.Queries;

public sealed record GetWebhooksQuery(string UserId) : IAppRequest<Model.Results.Result<List<WebhookDto>>>;