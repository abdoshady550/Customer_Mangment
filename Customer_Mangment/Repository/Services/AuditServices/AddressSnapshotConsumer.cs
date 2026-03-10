using Customer_Mangment.Model.Entities.History;
using Customer_Mangment.Model.Events;
using Customer_Mangment.Repository.Interfaces.MassageBroker;
using MongoDB.Driver;

namespace Customer_Mangment.Repository.Services.AuditServices
{
    public sealed class AddressSnapshotConsumer(
            IMongoCollection<AddressSnapshot> snapshots,
            ILogger<AddressSnapshotConsumer> logger) : IMessageConsumer<AddressSnapshotMessage>
    {
        public async Task ConsumeAsync(AddressSnapshotMessage message, CancellationToken ct = default)
        {
            logger.LogInformation(
                "Saving address snapshot. AddressId={Id} Operation={Op}",
                message.AddressId, message.Operation);

            var snapshot = new AddressSnapshot
            {
                AddressId = message.AddressId,
                CustomerId = message.CustomerId,
                Type = message.Type,
                Value = message.Value,
                ValidFrom = DateTime.UtcNow,
                Operation = message.Operation
            };

            await snapshots.InsertOneAsync(snapshot, cancellationToken: ct);

            logger.LogInformation(
                "Address snapshot saved. AddressId={Id}", message.AddressId);
        }
    }
}
