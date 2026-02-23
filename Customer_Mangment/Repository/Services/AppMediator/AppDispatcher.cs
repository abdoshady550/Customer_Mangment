using Customer_Mangment.Repository.Interfaces.AppMediator;
using MediatR;

namespace Customer_Mangment.Repository.Services.AppMediator
{
    public class AppDispatcher(IMediator mediator) : IDispatcher
    {
        private readonly IMediator _mediator = mediator;

        public Task<TResponse> Send<TResponse>(IAppRequest<TResponse> request, CancellationToken cancellationToken = default)
            => _mediator.Send(request, cancellationToken);

    }
}
