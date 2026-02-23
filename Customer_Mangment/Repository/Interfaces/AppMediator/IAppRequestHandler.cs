using MediatR;

namespace Customer_Mangment.Repository.Interfaces.AppMediator
{
    public interface IAppRequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse> where TRequest : IAppRequest<TResponse>
    {

    }
}
