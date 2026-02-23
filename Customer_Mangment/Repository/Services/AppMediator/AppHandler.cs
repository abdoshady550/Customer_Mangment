using Customer_Mangment.Repository.Interfaces.AppMediator;
using MediatR;

namespace Customer_Mangment.Repository.Services.AppMediator
{
    public class AppHandler<TRequest, TResponse>(IAppRequestHandler<TRequest, TResponse> handler)
        : IRequestHandler<AppRequest<TResponse>, TResponse> where TRequest : IAppRequest<TResponse>
    {
        private readonly IAppRequestHandler<TRequest, TResponse> _handler = handler;

        public Task<TResponse> Handle(AppRequest<TResponse> request, CancellationToken cancellationToken = default)
            => _handler.Handle((TRequest)request.appRequest, cancellationToken);
    }
}
