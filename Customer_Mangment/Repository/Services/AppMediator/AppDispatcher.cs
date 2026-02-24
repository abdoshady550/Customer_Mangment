using Customer_Mangment.Repository.Interfaces.AppMediator;
using Wolverine;

namespace Customer_Mangment.Repository.Services.AppMediator
{
    public class AppDispatcher(IMessageBus bus) : IDispatcher
    {
        private readonly IMessageBus _bus = bus;

        public Task<TResponse> Send<TResponse>(IAppRequest<TResponse> request, CancellationToken cancellationToken = default)
            => _bus.InvokeAsync<TResponse>(request, cancellationToken);
    }
}
