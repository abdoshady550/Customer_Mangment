using Customer_Mangment.Model.Results;
using MediatR;

namespace Customer_Mangment.CQRS.Customers.Commands.UpdateCustomer
{
    public sealed record UpdateCustomerCommand(string UserId, Guid CustomerId, string? Name, string? Mobile) : IRequest<Result<Updated>>;
}
