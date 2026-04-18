using Wolverine;

namespace Customer_Mangment.IdentityServer.CQRS
{
    public sealed class IdentityDispatcher(IMessageBus bus) : IIdentityDispatcher
    {
        private readonly IMessageBus _bus = bus;

        public Task<TResponse> Send<TResponse>(
            IIdentityRequest<TResponse> request,
            CancellationToken ct = default)
            => _bus.InvokeAsync<TResponse>(request, ct);
    }
}
