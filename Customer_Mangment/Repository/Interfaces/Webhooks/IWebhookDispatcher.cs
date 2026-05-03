namespace Customer_Mangment.Repository.Interfaces.Webhooks;

public interface IWebhookDispatcher
{
    Task DispatchAsync(string eventName, object data, CancellationToken ct = default);
}