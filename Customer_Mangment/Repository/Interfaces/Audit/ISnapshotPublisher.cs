using Customer_Mangment.Model.Entities;

namespace Customer_Mangment.Repository.Interfaces.Audit
{
    public interface ISnapshotPublisher
    {
        Task PublishCustomerSnapshotAsync(Customer customer, string operation, CancellationToken ct = default);
        Task PublishAddressSnapshotAsync(Address address, string operation, CancellationToken ct = default);
    }
}
