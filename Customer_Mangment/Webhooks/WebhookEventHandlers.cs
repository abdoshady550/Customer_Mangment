using Customer_Mangment.Model.Events;
using Customer_Mangment.Repository.Interfaces.Webhooks;

namespace Customer_Mangment.Webhooks;

public sealed class WebhookEventHandlers(IWebhookDispatcher dispatcher, ILogger<WebhookEventHandlers> logger)
{
    public async Task Handle(CustomerCreatedEvent e, CancellationToken ct)
    {
        logger.LogDebug("Webhook trigger: {Event}", WebhookEventNames.CustomerCreated);

        await dispatcher.DispatchAsync(WebhookEventNames.CustomerCreated, new { e.Customer.Id, e.Customer.Name, e.Customer.Mobile }, ct);
    }

    public async Task Handle(CustomerUpdatedEvent e, CancellationToken ct)
    {
        await dispatcher.DispatchAsync(WebhookEventNames.CustomerUpdated, new { e.Customer.Id, e.Customer.Name, e.Customer.Mobile }, ct);
    }

    public async Task Handle(CustomerDeletedEvent e, CancellationToken ct)
    {
        await dispatcher.DispatchAsync(WebhookEventNames.CustomerDeleted, new { e.Customer.Id }, ct);
    }

    public async Task Handle(AddressCreatedEvent e, CancellationToken ct)
    {
        await dispatcher.DispatchAsync(WebhookEventNames.AddressCreated, new { e.Address.Id, e.Address.CustomerId, e.Address.Type, e.Address.Value }, ct);
    }

    public async Task Handle(AddressUpdatedEvent e, CancellationToken ct)
    {
        await dispatcher.DispatchAsync(WebhookEventNames.AddressUpdated, new { e.Address.Id, e.Address.CustomerId, e.Address.Type, e.Address.Value }, ct);
    }

    public async Task Handle(AddressDeletedEvent e, CancellationToken ct)
    {
        await dispatcher.DispatchAsync(WebhookEventNames.AddressDeleted, new { e.Address.Id, e.Address.CustomerId }, ct);
    }
}