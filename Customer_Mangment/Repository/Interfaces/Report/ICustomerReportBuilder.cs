using Customer_Mangment.CQRS.Customers.Queries.Report.Dtos;

namespace Customer_Mangment.Repository.Interfaces.Report
{
    public interface ICustomerReportBuilder
    {

        byte[] Build(IReadOnlyList<CustomerReportRow> rows, DateTime? from, DateTime? to);
    }

}
