using Customer_Mangment.CQRS.Customers.Addresses.DTOS;
using Customer_Mangment.Repository.Interfaces.AppMediator;

namespace Customer_Mangment.CQRS.Customers.Addresses.Queries.OData
{
    public sealed record ODataAddressesQuery(string UserId, Guid? CustomerId) : IAppRequest<Model.Results.Result<IQueryable<AddressDto>>>;
}
