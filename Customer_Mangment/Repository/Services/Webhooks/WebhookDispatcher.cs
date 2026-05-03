using Customer_Mangment.Data;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.Repository.Interfaces.Webhooks;
using Customer_Mangment.Webhooks;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Customer_Mangment.Repository.Services.Webhooks;

public sealed class WebhookDispatcher(
    IHttpClientFactory httpClientFactory,
    IServiceScopeFactory scopeFactory,
    ILogger<WebhookDispatcher> logger) : IWebhookDispatcher
{
    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task DispatchAsync(string eventName, object data, CancellationToken ct = default)
    {
        List<WebhookSubscription> subscriptions;

        await using (var scope = scopeFactory.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            subscriptions = await db.WebhookSubscriptions
                .Where(s => s.IsActive)
                .ToListAsync(ct);

            subscriptions = subscriptions
                .Where(s => s.SubscribesTo(eventName))
                .ToList();
        }

        if (subscriptions.Count == 0)
        {
            logger.LogDebug(
                "No active webhook subscriptions for event {Event}", eventName);
            return;
        }

        var payload = new WebhookPayload(EventType: eventName, EventId: Guid.NewGuid().ToString(), OccurredAt: DateTime.UtcNow, Data: data);

        var payloadJson = JsonSerializer.Serialize(payload, _json);

        var tasks = subscriptions.Select(sub => SendToSubscriberAsync(sub, payloadJson, ct));

        await Task.WhenAll(tasks);
    }

    private async Task SendToSubscriberAsync(WebhookSubscription sub, string payloadJson, CancellationToken ct)
    {
        try
        {
            var signature = ComputeSignature(payloadJson, sub.Secret);

            var request = new HttpRequestMessage(HttpMethod.Post, sub.Url)
            {
                Content = new StringContent(payloadJson, Encoding.UTF8, "application/json")
            };

            request.Headers.Add("X-Webhook-Signature", $"sha256={signature}");
            request.Headers.Add("X-Webhook-Event", sub.Events);
            request.Headers.Add("X-Delivery-Id", Guid.NewGuid().ToString());

            var client = httpClientFactory.CreateClient("WebhookClient");
            var response = await client.SendAsync(request, ct);

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation(
                    "Webhook delivered. Event={Event} Url={Url} Status={Status}",
                    sub.Events, sub.Url, (int)response.StatusCode);
            }
            else
            {
                logger.LogWarning(
                    "Webhook delivery failed. Event={Event} Url={Url} Status={Status}",
                    sub.Events, sub.Url, (int)response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Webhook dispatch threw. SubscriptionId={Id} Url={Url}",
                sub.Id, sub.Url);
        }
    }
    private static string ComputeSignature(string payload, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        var hash = HMACSHA256.HashData(keyBytes, payloadBytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
