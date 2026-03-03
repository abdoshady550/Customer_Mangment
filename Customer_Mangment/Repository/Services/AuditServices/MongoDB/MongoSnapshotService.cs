using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Entities.History;
using Customer_Mangment.Model.Events;
using MongoDB.Driver;

namespace Customer_Mangment.Repository.Services.AuditServices.MongoDB
{
    public sealed class MongoSnapshotService(
        IMongoCollection<CustomerSnapshot> customerSnapshots,
        IMongoCollection<AddressSnapshot> addressSnapshots)
    {
        // --- Customer events ---

        public async Task Handle(CustomerCreatedEvent e, CancellationToken ct)
            => await SaveCustomerSnapshot(e.Customer, "Created", ct);

        public async Task Handle(CustomerUpdatedEvent e, CancellationToken ct)
            => await SaveCustomerSnapshot(e.Customer, "Updated", ct);

        public async Task Handle(CustomerDeletedEvent e, CancellationToken ct)
            => await SaveCustomerSnapshot(e.Customer, "Deleted", ct);

        // --- Address events ---

        public async Task Handle(AddressCreatedEvent e, CancellationToken ct)
            => await SaveAddressSnapshot(e.Address, "Created", ct);

        public async Task Handle(AddressUpdatedEvent e, CancellationToken ct)
            => await SaveAddressSnapshot(e.Address, "Updated", ct);

        public async Task Handle(AddressDeletedEvent e, CancellationToken ct)
            => await SaveAddressSnapshot(e.Address, "Deleted", ct);

        private async Task SaveCustomerSnapshot(
            Customer customer,
            string operation,
            CancellationToken ct)
        {
            var snapshot = new CustomerSnapshot
            {
                CustomerId = customer.Id,
                Name = customer.Name,
                Mobile = customer.Mobile,
                CreatedBy = customer.CreatedBy,
                UpdatedBy = customer.UpdatedBy,
                IsDeleted = customer.IsDeleted,
                ValidFrom = DateTime.UtcNow,
                Operation = operation
            };

            await customerSnapshots.InsertOneAsync(snapshot, cancellationToken: ct);
        }

        private async Task SaveAddressSnapshot(
            Address address,
            string operation,
            CancellationToken ct)
        {
            var snapshot = new AddressSnapshot
            {
                AddressId = address.Id,
                CustomerId = address.CustomerId,
                Type = address.Type,
                Value = address.Value,
                ValidFrom = DateTime.UtcNow,
                Operation = operation
            };

            await addressSnapshots.InsertOneAsync(snapshot, cancellationToken: ct);
        }
    }
}
