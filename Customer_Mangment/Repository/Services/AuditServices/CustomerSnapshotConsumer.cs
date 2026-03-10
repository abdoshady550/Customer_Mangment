using Customer_Mangment.Model.Entities.History;
using Customer_Mangment.Model.Events;
using Customer_Mangment.Repository.Interfaces.MassageBroker;
using MongoDB.Driver;

namespace Customer_Mangment.Repository.Services.AuditServices
{
    public sealed class CustomerSnapshotConsumer(
           IMongoCollection<CustomerSnapshot> snapshots,
           ILogger<CustomerSnapshotConsumer> logger)
           : IMessageConsumer<CustomerSnapshotMessage>
    {
        public async Task ConsumeAsync(CustomerSnapshotMessage message, CancellationToken ct = default)
        {
            logger.LogInformation(
                "Saving customer snapshot. CustomerId={Id} Operation={Op}",
                message.CustomerId, message.Operation);

            var snapshot = new CustomerSnapshot
            {
                CustomerId = message.CustomerId,
                Name = message.Name,
                Mobile = message.Mobile,
                CreatedBy = message.CreatedBy,
                UpdatedBy = message.UpdatedBy,
                IsDeleted = message.IsDeleted,
                ValidFrom = DateTime.UtcNow,
                Operation = message.Operation
            };

            await snapshots.InsertOneAsync(snapshot, cancellationToken: ct);

            logger.LogInformation(
                "Customer snapshot saved. CustomerId={Id}", message.CustomerId);
        }
    }
}
