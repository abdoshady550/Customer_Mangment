using Customer_Mangment.CQRS.Webhooks.DTOs;
using Customer_Mangment.Data;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.Repository.Interfaces.AppMediator;
using Customer_Mangment.SharedResources;
using Customer_Mangment.SharedResources.Keys;
using Microsoft.Extensions.Localization;

namespace Customer_Mangment.CQRS.Webhooks.Commands;

public sealed class CreateWebhookHandler(AppDbContext db, IStringLocalizer<SharedResource> localizer, ILogger<CreateWebhookHandler> logger)
    : IAppRequestHandler<CreateWebhookCommand, Model.Results.Result<WebhookDto>>
{
    public async Task<Model.Results.Result<WebhookDto>> Handle(CreateWebhookCommand request, CancellationToken ct)
    {
        var user = await db.Users.FindAsync([request.UserId], ct);
        if (user is null)
            return LocalizedError.Unauthorized(localizer, "UserNotFound", ResourceKeys.User.NotFound, request.UserId);

        var subscription = WebhookSubscription.Create(request.Url, request.Events, user.UserName!);

        db.WebhookSubscriptions.Add(subscription);

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Webhook created. Id={Id} Url={Url} Events={Events}", subscription.Id, subscription.Url, subscription.Events);

        return new WebhookDto(
            subscription.Id,
            subscription.Url,
            subscription.Events.Split(','),
            subscription.IsActive,
            subscription.CreatedBy,
            subscription.CreatedAt,
            Secret: subscription.Secret);
    }
}