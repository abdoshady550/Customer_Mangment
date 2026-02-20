using Customer_Mangment.Model.Results;
using MediatR;

namespace Customer_Mangment.CQRS.Customers.Addresses.Commands.DeleteAddress
{
    public sealed record DeleteAddressCommand(string UserId, Guid AddressId) : IRequest<Result<Deleted>>;
}
