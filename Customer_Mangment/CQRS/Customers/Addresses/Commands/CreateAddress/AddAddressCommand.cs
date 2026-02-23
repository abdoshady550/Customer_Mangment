using Customer_Mangment.CQRS.Customers.Addresses.DTOS;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces.AppMediator;

namespace Customer_Mangment.CQRS.Customers.Addresses.Commands.CreateAddress
{
    public sealed record AddAddressCommand(string UserId, Guid CustomerId, AdressType Type, string Value) : IAppRequest<Result<AddressDto>>;

}
