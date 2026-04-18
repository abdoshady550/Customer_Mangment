namespace Customer_Mangment.IdentityServer.CQRS
{
    public interface IIdentityRequest<TResponse> { }

    public interface IIdentityRequestHandler<TRequest, TResponse>
        where TRequest : IIdentityRequest<TResponse>
    {
        Task<TResponse> Handle(TRequest request, CancellationToken ct = default);
    }

    public interface IIdentityDispatcher
    {
        Task<TResponse> Send<TResponse>(
            IIdentityRequest<TResponse> request,
            CancellationToken ct = default);
    }
}