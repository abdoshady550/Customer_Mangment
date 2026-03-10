namespace Customer_Mangment.Repository.Interfaces.MassageBroker
{
    public sealed class ConsumerAdapter<TMessage>(IMessageConsumer<TMessage> consumer,
                                                  ILogger<ConsumerAdapter<TMessage>> logger) where TMessage : class
    {
        public async Task HandleAsync(TMessage message, CancellationToken ct)
        {
            logger.LogDebug(
                "Dispatching {MessageType} to {ConsumerType}",
                typeof(TMessage).Name,
                consumer.GetType().Name);

            await consumer.ConsumeAsync(message, ct);
        }
    }
}
