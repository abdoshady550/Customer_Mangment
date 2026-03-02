using Customer_Mangment.Model.Entities;
using Customer_Mangment.Repository.Interfaces.Audit;

namespace Customer_Mangment.Repository.Services.AuditServices
{
    public sealed class SqlSnapshotService : ISnapshotService
    {
        public Task SaveCustomerSnapshotAsync(Customer customer, string operation, CancellationToken ct = default)
            => Task.CompletedTask;

        public Task SaveAddressSnapshotAsync(Address address, string operation, CancellationToken ct = default)
            => Task.CompletedTask;
    }
}
