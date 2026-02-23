using MediatR;

namespace Customer_Mangment.Repository.Interfaces.AppMediator
{
    public interface IAppRequest<TResponse> : IRequest<TResponse> { }
}
