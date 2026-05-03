namespace Customer_Mangment.CQRS.Webhooks.DTOs;

public sealed record WebhookDto(Guid Id, string Url, string[] Events, bool IsActive, string CreatedBy, DateTime CreatedAt, string? Secret = null);