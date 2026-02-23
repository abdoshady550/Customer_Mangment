namespace Customer_Mangment.Repository.Interfaces.AppMediator
{
    public interface IDispatcher
    {
        Task<TResponse> Send<TResponse>(IAppRequest<TResponse> request, CancellationToken cancellationToken = default);
    }
}
