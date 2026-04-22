using Customer_Mangment.CQRS.Customers.DTOS;
using Customer_Mangment.Repository.Interfaces.AppMediator;

namespace Customer_Mangment.CQRS.Customers.Queries.GetCustomers
{
    public sealed record GetAddressHistoryQuery(string UserId, Guid CustomerId) : IAppRequest<Model.Results.Result<List<AddressHistoryDto>>>;

}
