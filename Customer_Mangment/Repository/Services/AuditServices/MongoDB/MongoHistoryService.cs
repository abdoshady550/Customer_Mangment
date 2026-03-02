using Customer_Mangment.CQRS.Customers.Addresses.DTOS;
using Customer_Mangment.CQRS.Customers.DTOS;
using Customer_Mangment.Model.Entities.History;
using Customer_Mangment.Repository.Interfaces.Audit;
using MongoDB.Driver;

namespace Customer_Mangment.Repository.Services.AuditServices.MongoDB
{
    public sealed class MongoHistoryService(
        IMongoCollection<CustomerSnapshot> customerSnapshots,
        IMongoCollection<AddressSnapshot> addressSnapshots) : IHistoryService
    {
        public async Task<List<CustomerDto>> GetCustomerHistoryAsync(Guid customerId, CancellationToken ct = default)
        {
            var snapshots = await customerSnapshots
                .Find(s => s.CustomerId == customerId)
                .SortBy(s => s.ValidFrom)
                .ToListAsync(ct);

            return snapshots.Select(s => new CustomerDto(
                s.CustomerId,
                s.Name,
                s.Mobile,
                s.CreatedBy,
                s.UpdatedBy,
                []
            )).ToList();
        }

        public async Task<List<AddressDto>> GetAddressHistoryAsync(Guid customerId, CancellationToken ct = default)
        {
            var snapshots = await addressSnapshots
                .Find(s => s.CustomerId == customerId)
                .SortBy(s => s.ValidFrom)
                .ToListAsync(ct);

            return snapshots.Select(s => new AddressDto(
                s.AddressId,
                s.CustomerId,
                s.Type,
                s.Value
            )).ToList();
        }
    }
}
