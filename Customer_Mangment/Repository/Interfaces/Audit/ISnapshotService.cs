using Customer_Mangment.Model.Entities;

namespace Customer_Mangment.Repository.Interfaces.Audit
{
    public interface ISnapshotService
    {
        Task SaveCustomerSnapshotAsync(Customer customer, string operation, CancellationToken ct = default);
        Task SaveAddressSnapshotAsync(Address address, string operation, CancellationToken ct = default);
    }
}
