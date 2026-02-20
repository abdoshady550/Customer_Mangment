using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Results;
using MediatR;

namespace Customer_Mangment.CQRS.Customers.Addresses.Commands.UpdateAddress
{
    public sealed record UpdateAddressCommand(string UserId, Guid AddressId, AdressType? Type, string? Value) : IRequest<Result<Updated>>;
}
