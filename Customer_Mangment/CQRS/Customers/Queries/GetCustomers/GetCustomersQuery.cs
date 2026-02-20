using Customer_Mangment.CQRS.Customers.DTOS;
using Customer_Mangment.Model.Results;
using MediatR;

namespace Customer_Mangment.CQRS.Customers.Queries.GetCustomers
{
    public sealed record GetCustomersQuery(string UserId, Guid? CustomerId) : IRequest<Result<List<CustomerDto>>>;

}
