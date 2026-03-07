using Customer_Mangment.Model.Entities.History;
using Customer_Mangment.Model.Events;
using MassTransit;
using MongoDB.Driver;

namespace Customer_Mangment.Repository.Services.AuditServices
{
    public sealed class AddressSnapshotConsumer(
    IMongoCollection<AddressSnapshot> snapshots,
    ILogger<AddressSnapshotConsumer> logger) : IConsumer<AddressSnapshotMessage>
    {
        public async Task Consume(ConsumeContext<AddressSnapshotMessage> context)
        {
            var msg = context.Message;

            logger.LogInformation(
                "Saving address snapshot. AddressId={Id} Operation={Op}",
                msg.AddressId, msg.Operation);

            var snapshot = new AddressSnapshot
            {
                AddressId = msg.AddressId,
                CustomerId = msg.CustomerId,
                Type = msg.Type,
                Value = msg.Value,
                ValidFrom = DateTime.UtcNow,
                Operation = msg.Operation
            };

            await snapshots.InsertOneAsync(snapshot, cancellationToken: context.CancellationToken);

            logger.LogInformation(
                "Address snapshot saved. AddressId={Id}", msg.AddressId);
        }
    }
}
