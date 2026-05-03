using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Customer_Mangment.Model.Entities;

public sealed class WebhookSubscription
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; private set; }

    public string Url { get; private set; } = string.Empty;


    public string Events { get; private set; } = string.Empty;

    public string Secret { get; private set; } = string.Empty;

    public bool IsActive { get; private set; } = true;

    public string CreatedBy { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }

    private WebhookSubscription() { }

    public static WebhookSubscription Create(string url, IEnumerable<string> events, string createdBy)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL is required.", nameof(url));

        return new WebhookSubscription
        {
            Id = Guid.NewGuid(),
            Url = url.Trim(),
            Events = string.Join(",", events.Select(e => e.Trim().ToLowerInvariant())),
            Secret = GenerateSecret(),
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }

    public void Deactivate() => IsActive = false;

    public bool SubscribesTo(string eventName)
        => IsActive && Events.Split(',').Any(e => e.Equals(eventName, StringComparison.OrdinalIgnoreCase));

    private static string GenerateSecret() => Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
}