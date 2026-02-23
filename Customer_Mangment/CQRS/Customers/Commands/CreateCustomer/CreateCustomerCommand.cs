using Customer_Mangment.CQRS.Customers.Addresses.Commands.CreateAddress;
using Customer_Mangment.CQRS.Customers.DTOS;
using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces.AppMediator;

namespace Customer_Mangment.CQRS.Customers.Commands.CreateCustomer
{
    public sealed record CreateCustomerCommand(string UserId,
                                               string Name,
                                               string Mobile,
                                               List<CreateAddressCommand> Adresses) : IAppRequest<Result<CustomerDto>>;
}
