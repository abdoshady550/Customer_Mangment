using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces.AppMediator;

namespace Customer_Mangment.CQRS.Customers.Addresses.Commands.DeleteAddress
{
    public sealed record DeleteAddressCommand(string UserId, Guid AddressId) : IAppRequest<Result<Deleted>>;
}
