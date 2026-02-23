using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces.AppMediator;

namespace Customer_Mangment.CQRS.Customers.Commands.DeleteCustomer
{
    public sealed record DeleteCustomerCommand(string UserId, Guid CustomerId) : IAppRequest<Result<Deleted>>;
}
