using Customer_Mangment.CQRS.Customers.DTOS;

namespace Customer_Mangment.Repository.Interfaces.Audit
{
    public interface IHistoryService
    {
        Task<List<CustomerHistoryDto>> GetCustomerHistoryAsync(Guid customerId, CancellationToken ct = default);
        Task<List<AddressHistoryDto>> GetAddressHistoryAsync(Guid customerId, CancellationToken ct = default);
    }
}
