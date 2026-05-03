namespace Customer_Mangment.Webhooks;

public sealed record WebhookPayload(string EventType, string EventId, DateTime OccurredAt, object Data);