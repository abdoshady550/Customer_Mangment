using Customer_Mangment.CQRS.Customers.DTOS;
using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces.AppMediator;

namespace Customer_Mangment.CQRS.Customers.Queries.GetCustomers
{
    public sealed record GetCustomerHistoryQuery(string UserId, Guid CustomerId) : IAppRequest<Result<List<CustomerDto>>>;
}
