using Customer_Mangment.CQRS.Webhooks.DTOs;
using Customer_Mangment.Data;
using Customer_Mangment.Repository.Interfaces.AppMediator;
using Customer_Mangment.SharedResources;
using Customer_Mangment.SharedResources.Keys;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Customer_Mangment.CQRS.Webhooks.Queries;

public sealed class GetWebhooksHandler(AppDbContext db, IStringLocalizer<SharedResource> localizer)
    : IAppRequestHandler<GetWebhooksQuery, Model.Results.Result<List<WebhookDto>>>
{
    public async Task<Model.Results.Result<List<WebhookDto>>> Handle(GetWebhooksQuery request, CancellationToken ct)
    {
        var user = await db.Users.FindAsync([request.UserId], ct);
        if (user is null)
            return LocalizedError.Unauthorized(localizer, "UserNotFound", ResourceKeys.User.NotFound, request.UserId);

        var subs = await db.WebhookSubscriptions
            .AsNoTracking()
            .Where(s => s.IsActive)
            .ToListAsync(ct);

        return subs.Select(s => new WebhookDto(
            s.Id,
            s.Url,
            s.Events.Split(','),
            s.IsActive,
            s.CreatedBy,
            s.CreatedAt
            )).ToList();
    }
}