using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces.AppMediator;

namespace Customer_Mangment.CQRS.Customers.Commands.UpdateCustomer
{
    public sealed record UpdateCustomerCommand(string UserId, Guid CustomerId, string? Name, string? Mobile) : IAppRequest<Result<Updated>>;
}
