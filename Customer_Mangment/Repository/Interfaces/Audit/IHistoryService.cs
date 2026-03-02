using Customer_Mangment.CQRS.Customers.Addresses.DTOS;
using Customer_Mangment.CQRS.Customers.DTOS;

namespace Customer_Mangment.Repository.Interfaces.Audit
{
    public interface IHistoryService
    {
        Task<List<CustomerDto>> GetCustomerHistoryAsync(Guid customerId, CancellationToken ct = default);
        Task<List<AddressDto>> GetAddressHistoryAsync(Guid customerId, CancellationToken ct = default);
    }
}
