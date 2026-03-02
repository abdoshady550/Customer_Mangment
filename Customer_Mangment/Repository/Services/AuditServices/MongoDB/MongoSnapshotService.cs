using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Entities.History;
using Customer_Mangment.Repository.Interfaces.Audit;
using MongoDB.Driver;

namespace Customer_Mangment.Repository.Services.AuditServices.MongoDB
{
    public sealed class MongoSnapshotService(
        IMongoCollection<CustomerSnapshot> customerSnapshots,
        IMongoCollection<AddressSnapshot> addressSnapshots) : ISnapshotService
    {

        public async Task SaveCustomerSnapshotAsync(Customer customer, string operation, CancellationToken ct = default)
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

        public async Task SaveAddressSnapshotAsync(Address address, string operation, CancellationToken ct = default)
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
