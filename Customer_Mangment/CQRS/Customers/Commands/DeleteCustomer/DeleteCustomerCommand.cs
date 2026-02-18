using Customer_Mangment.Model.Results;
using MediatR;

namespace Customer_Mangment.CQRS.Customers.Commands.DeleteCustomer
{
    public sealed record DeleteCustomerCommand(int UserId, Guid CustomerId) : IRequest<Result<Deleted>>;
}
