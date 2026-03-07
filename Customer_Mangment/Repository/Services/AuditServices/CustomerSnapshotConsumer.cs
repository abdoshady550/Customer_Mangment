using Customer_Mangment.Model.Entities.History;
using Customer_Mangment.Model.Events;
using MassTransit;
using MongoDB.Driver;

namespace Customer_Mangment.Repository.Services.AuditServices
{
    public sealed class CustomerSnapshotConsumer(
        IMongoCollection<CustomerSnapshot> snapshots,
        ILogger<CustomerSnapshotConsumer> logger) : IConsumer<CustomerSnapshotMessage>
    {
        public async Task Consume(ConsumeContext<CustomerSnapshotMessage> context)
        {
            var msg = context.Message;

            logger.LogInformation(
                "Saving customer snapshot. CustomerId={Id} Operation={Op}",
                msg.CustomerId, msg.Operation);

            var snapshot = new CustomerSnapshot
            {
                CustomerId = msg.CustomerId,
                Name = msg.Name,
                Mobile = msg.Mobile,
                CreatedBy = msg.CreatedBy,
                UpdatedBy = msg.UpdatedBy,
                IsDeleted = msg.IsDeleted,
                ValidFrom = DateTime.UtcNow,
                Operation = msg.Operation
            };

            await snapshots.InsertOneAsync(snapshot, cancellationToken: context.CancellationToken);

            logger.LogInformation(
                "Customer snapshot saved. CustomerId={Id}", msg.CustomerId);
        }
    }
}
