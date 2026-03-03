using Customer_Mangment.CQRS.Customers.DTOS;
using Customer_Mangment.CQRS.Customers.Mappers;
using Customer_Mangment.Model.Entities.History;
using Customer_Mangment.Repository.Interfaces.Audit;
using MongoDB.Driver;

namespace Customer_Mangment.Repository.Services.AuditServices.MongoDB
{
    public sealed class MongoHistoryService(
        IMongoCollection<CustomerSnapshot> customerSnapshots,
        IMongoCollection<AddressSnapshot> addressSnapshots,
        ICustomerMapper mapper) : IHistoryService
    {
        public async Task<List<CustomerHistoryDto>> GetCustomerHistoryAsync(Guid customerId, CancellationToken ct = default)
        {
            var snapshots = await customerSnapshots
                .Find(s => s.CustomerId == customerId)
                .SortBy(s => s.ValidFrom)
                .ToListAsync(ct);

            return mapper.ToCustomerHistoryDtoList(snapshots);
        }

        public async Task<List<AddressHistoryDto>> GetAddressHistoryAsync(Guid customerId, CancellationToken ct = default)
        {
            var snapshots = await addressSnapshots
                .Find(s => s.CustomerId == customerId)
                .SortBy(s => s.ValidFrom)
                .ToListAsync(ct);

            return mapper.ToAddressHistoryDtoList(snapshots);
        }
    }
}
