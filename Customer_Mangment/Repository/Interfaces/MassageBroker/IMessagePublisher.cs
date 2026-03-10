namespace Customer_Mangment.Repository.Interfaces.MassageBroker
{
    public interface IMessagePublisher
    {
        Task PublishAsync<TMessage>(TMessage message, CancellationToken ct = default) where TMessage : class;
    }
}
