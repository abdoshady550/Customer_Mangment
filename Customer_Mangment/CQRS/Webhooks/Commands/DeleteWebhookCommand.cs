using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces.AppMediator;

namespace Customer_Mangment.CQRS.Webhooks.Commands;

public sealed record DeleteWebhookCommand(string UserId, Guid WebhookId) : IAppRequest<Model.Results.Result<Deleted>>;