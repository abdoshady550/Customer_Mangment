using Customer_Mangment.CQRS.Customers.Addresses.DTOS;
using Customer_Mangment.Repository.Interfaces.AppMediator;

namespace Customer_Mangment.CQRS.Customers.Queries.GetCustomers
{
    public sealed record GetAddressQuery(string UserId, Guid? CustomerId, Guid? AddressId) : IAppRequest<Model.Results.Result<List<AddressDto>>>;

}
