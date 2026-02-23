using Customer_Mangment.Repository.Interfaces.AppMediator;
using MediatR;

namespace Customer_Mangment.Repository.Services.AppMediator
{
    public class AppRequest<TResponse>(IAppRequest<TResponse> request) : IRequest<TResponse>
    {
        public IAppRequest<TResponse> appRequest { get; } = request;
    }
}
