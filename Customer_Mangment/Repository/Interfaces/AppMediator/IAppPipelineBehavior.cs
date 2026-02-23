using MediatR;

namespace Customer_Mangment.Repository.Interfaces.AppMediator
{
    public interface IAppPipelineBehavior<TReq, TRes> where TReq : IAppRequest<TReq>
    {
        Task<TRes> Handle(TReq request, RequestHandlerDelegate<TRes> next, CancellationToken cancellationToken = default);
    }
}
