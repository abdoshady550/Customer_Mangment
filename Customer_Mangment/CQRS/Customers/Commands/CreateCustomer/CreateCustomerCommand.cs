using Customer_Mangment.CQRS.Customers.Addresses.Commands.CreateAddress;
using Customer_Mangment.CQRS.Customers.DTOS;
using Customer_Mangment.Model.Results;
using MediatR;

namespace Customer_Mangment.CQRS.Customers.Commands.CreateCustomer
{
    public sealed record CreateCustomerCommand(int userId,
                                               string Name,
                                               string Mobile,
                                               List<CreateAddressCommand> Adresses) : IRequest<Result<CustomerDto>>;
}
