using Customer_Mangment.CQRS.Customers.Addresses.DTOS;
using Customer_Mangment.CQRS.Customers.DTOS;
using Customer_Mangment.CQRS.Customers.Mappers;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.Repository.Interfaces;
using Customer_Mangment.Repository.Interfaces.Audit;

namespace Customer_Mangment.Repository.Services.AuditServices
{
    public sealed class SqlHistoryService(
           IGenericRepo<Customer> customerRepo,
           IGenericRepo<Address> addressRepo,
           ICustomerMapper mapper) : IHistoryService
    {
        public async Task<List<CustomerDto>> GetCustomerHistoryAsync(Guid customerId, CancellationToken ct = default)
        {
            var history = await customerRepo
                .TemporalAll()
                .Where(c => c.Id == customerId)
                .ToListAsync(ct);

            return mapper.ToCustomerDtoList(history);
        }

        public async Task<List<AddressDto>> GetAddressHistoryAsync(Guid customerId, CancellationToken ct = default)
        {
            var history = await addressRepo
                .TemporalAll()
                .Where(a => a.CustomerId == customerId)
                .ToListAsync(ct);

            return mapper.ToAddressDtoList(history);
        }
    }
}
