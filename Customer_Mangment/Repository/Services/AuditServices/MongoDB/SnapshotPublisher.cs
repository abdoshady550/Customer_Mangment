using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Events;
using Customer_Mangment.Repository.Interfaces.Audit;
using Customer_Mangment.Repository.Interfaces.MassageBroker;

namespace Customer_Mangment.Repository.Services.AuditServices.MongoDB
{
    public sealed class SnapshotPublisher(IMessagePublisher publisher) : ISnapshotPublisher
    {
        public Task PublishCustomerSnapshotAsync(
            Customer customer, string operation, CancellationToken ct = default)
            => publisher.PublishAsync(new CustomerSnapshotMessage(
                customer.Id,
                customer.Name,
                customer.Mobile,
                customer.CreatedBy,
                customer.UpdatedBy,
                customer.IsDeleted,
                operation), ct);

        public Task PublishAddressSnapshotAsync(
            Address address, string operation, CancellationToken ct = default)
            => publisher.PublishAsync(new AddressSnapshotMessage(
                address.Id,
                address.CustomerId,
                address.Type,
                address.Value,
                operation), ct);
    }
}
