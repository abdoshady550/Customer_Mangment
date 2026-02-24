namespace Customer_Mangment.Repository.Interfaces.AppMediator
{
    public interface IAppRequestHandler<TRequest, TResponse> where TRequest : IAppRequest<TResponse>
    {
        Task<TResponse> Handle(TRequest request, CancellationToken ct = default);
    }
}
