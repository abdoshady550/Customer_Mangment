namespace Customer_Mangment.CQRS.Customers.Queries.Report.Dtos
{
    public sealed record CustomerReportRow(
        Guid Id,
        string Name,
        string Mobile,
        DateTime CreatedAt,
        string CreatedBy,
        string UpdatedBy,
        bool IsDeleted);
}
