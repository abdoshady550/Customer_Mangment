using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces.AppMediator;

namespace Customer_Mangment.CQRS.Customers.Queries.Report
{
    public sealed record GetCustomerReportQuery(
        string UserId,
        DateTime? From,
        DateTime? To) : IAppRequest<Result<byte[]>>;
}
