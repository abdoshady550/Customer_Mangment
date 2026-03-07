using Customer_Mangment.Model.Events;
using Customer_Mangment.Repository.Interfaces.Audit;

namespace Customer_Mangment.Repository.Services.AuditServices.MongoDB
{
    public sealed class MongoSnapshotHandler(ISnapshotPublisher publisher)
    {
        public Task Handle(CustomerCreatedEvent e, CancellationToken ct)
            => publisher.PublishCustomerSnapshotAsync(e.Customer, "Created", ct);

        public Task Handle(CustomerUpdatedEvent e, CancellationToken ct)
            => publisher.PublishCustomerSnapshotAsync(e.Customer, "Updated", ct);

        public Task Handle(CustomerDeletedEvent e, CancellationToken ct)
            => publisher.PublishCustomerSnapshotAsync(e.Customer, "Deleted", ct);

        public Task Handle(AddressCreatedEvent e, CancellationToken ct)
            => publisher.PublishAddressSnapshotAsync(e.Address, "Created", ct);

        public Task Handle(AddressUpdatedEvent e, CancellationToken ct)
            => publisher.PublishAddressSnapshotAsync(e.Address, "Updated", ct);

        public Task Handle(AddressDeletedEvent e, CancellationToken ct)
            => publisher.PublishAddressSnapshotAsync(e.Address, "Deleted", ct);
    }
}
