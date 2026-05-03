using Customer_Mangment.Data;
using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces.AppMediator;
using Customer_Mangment.SharedResources;
using Customer_Mangment.SharedResources.Keys;
using Microsoft.Extensions.Localization;

namespace Customer_Mangment.CQRS.Webhooks.Commands;

public sealed class DeleteWebhookHandler(AppDbContext db, IStringLocalizer<SharedResource> localizer)
    : IAppRequestHandler<DeleteWebhookCommand, Model.Results.Result<Deleted>>
{
    public async Task<Model.Results.Result<Deleted>> Handle(
        DeleteWebhookCommand request, CancellationToken ct)
    {
        var user = await db.Users.FindAsync([request.UserId], ct);
        if (user is null)
            return LocalizedError.Unauthorized(localizer, "UserNotFound", ResourceKeys.User.NotFound, request.UserId);

        var subscription = await db.WebhookSubscriptions.FindAsync([request.WebhookId], ct);

        if (subscription is null)
            return LocalizedError.NotFound(localizer, "WebhookNotFound", ResourceKeys.General.NotFound);

        subscription.Deactivate();

        await db.SaveChangesAsync(ct);

        return Result.Deleted;
    }
}