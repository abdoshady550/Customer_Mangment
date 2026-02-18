using Customer_Mangment.CQRS.Customers.Addresses.DTOS;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Results;
using MediatR;

namespace Customer_Mangment.CQRS.Customers.Addresses.Commands.CreateAddress
{
    public sealed record CreateAddressCommand(AdressType Type, string Value) : IRequest<Result<AddressDto>>;
}
