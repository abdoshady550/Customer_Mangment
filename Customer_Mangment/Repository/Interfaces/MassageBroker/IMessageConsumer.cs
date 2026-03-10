namespace Customer_Mangment.Repository.Interfaces.MassageBroker
{

    public interface IMessageConsumer<in TMessage> where TMessage : class
    {
        Task ConsumeAsync(TMessage message, CancellationToken ct = default);
    }

}
