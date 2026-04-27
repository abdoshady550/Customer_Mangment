using Customer_Mangment.CQRS.Customers.DTOS;
using Customer_Mangment.Repository.Interfaces.AppMediator;

namespace Customer_Mangment.CQRS.Customers.Queries.OData
{
    public sealed record ODataCustomersQuery(string UserId) : IAppRequest<Model.Results.Result<IQueryable<CustomerDto>>>;
}
