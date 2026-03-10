using Customer_Mangment.Hubs;
using Customer_Mangment.Hubs.Models;
using Customer_Mangment.Model.Events;
using Customer_Mangment.Repository.Interfaces.Audit;
using Microsoft.AspNetCore.SignalR;

namespace Customer_Mangment.Repository.Services.AuditServices.MongoDB
{
    public sealed class MongoSnapshotHandler(ISnapshotPublisher publisher,
        IHubContext<QueueMonitorHub> hub,
        ILogger<MongoSnapshotHandler> logger)
    {
        public async Task Handle(CustomerCreatedEvent e, CancellationToken ct)
        {
            await publisher.PublishCustomerSnapshotAsync(e.Customer, "Created", ct);
            await BroadcastAsync("customer-snapshots", e.Customer.Id.ToString(),
                $"Customer '{e.Customer.Name}' created (Id={e.Customer.Id})", ct);
        }

        public async Task Handle(CustomerUpdatedEvent e, CancellationToken ct)
        {
            await publisher.PublishCustomerSnapshotAsync(e.Customer, "Updated", ct);

            await BroadcastAsync("customer-snapshots", e.Customer.Id.ToString(),
                $"Customer '{e.Customer.Name}' updated (Id={e.Customer.Id})", ct);
        }

        public async Task Handle(CustomerDeletedEvent e, CancellationToken ct)
        {
            await publisher.PublishCustomerSnapshotAsync(e.Customer, "Deleted", ct);

            await BroadcastAsync("customer-snapshots", e.Customer.Id.ToString(),
             $"Customer '{e.Customer.Name}' deleted (Id={e.Customer.Id})", ct);
        }

        public async Task Handle(AddressCreatedEvent e, CancellationToken ct)
        {
            await publisher.PublishAddressSnapshotAsync(e.Address, "Created", ct);
            await BroadcastAsync("address-snapshots", e.Address.Id.ToString(),
               $"Address created for CustomerId={e.Address.CustomerId} Type={e.Address.Type}", ct);
        }

        public async Task Handle(AddressUpdatedEvent e, CancellationToken ct)
        {
            await publisher.PublishAddressSnapshotAsync(e.Address, "Updated", ct);
            await BroadcastAsync("address-snapshots", e.Address.Id.ToString(),
                $"Address updated for CustomerId={e.Address.CustomerId} Type={e.Address.Type}", ct);
        }

        public async Task Handle(AddressDeletedEvent e, CancellationToken ct)
        {
            await publisher.PublishAddressSnapshotAsync(e.Address, "Deleted", ct);

            await BroadcastAsync("address-snapshots", e.Address.Id.ToString(),
                $"Address deleted for CustomerId={e.Address.CustomerId} Type={e.Address.Type}", ct);
        }

        private Task BroadcastAsync(string queue, string messageBody, string message, CancellationToken ct)
        {
            var notification = new QueueNotification(
                QueueName: queue,
                Status: QueueStatus.Received,
                Message: message,
                MessageBody: messageBody,
                ReceivedAt: DateTime.UtcNow);

            logger.LogInformation(
                "SignalR broadcast → Queue={Queue} Status={Status} Message={Message}",
                notification.QueueName, notification.Status, notification.Message);

            return hub.Clients.All.SendAsync("ReceiveQueueNotification", notification, ct);
        }
    }
}
