using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Events;
using Customer_Mangment.Repository.Interfaces.Audit;
using MassTransit;

namespace Customer_Mangment.Repository.Services.AuditServices.MongoDB
{
    public sealed class SnapshotPublisher(IPublishEndpoint publishEndpoint) : ISnapshotPublisher
    {
        public Task PublishCustomerSnapshotAsync(Customer customer, string operation, CancellationToken ct = default)
            => publishEndpoint.Publish(new CustomerSnapshotMessage(
                customer.Id,
                customer.Name,
                customer.Mobile,
                customer.CreatedBy,
                customer.UpdatedBy,
                customer.IsDeleted,
                operation), ct);

        public Task PublishAddressSnapshotAsync(Address address, string operation, CancellationToken ct = default)
            => publishEndpoint.Publish(new AddressSnapshotMessage(
                address.Id,
                address.CustomerId,
                address.Type,
                address.Value,
                operation), ct);
    }
}
