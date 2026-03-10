using Wolverine;

namespace Customer_Mangment.Repository.Interfaces.MassageBroker
{
    public sealed class MessagePublisher(IMessageBus bus) : IMessagePublisher
    {
        private readonly IMessageBus _bus = bus;

        public async Task PublishAsync<TMessage>(TMessage message, CancellationToken ct = default) where TMessage : class
            => await _bus.PublishAsync(message);
    }
}
