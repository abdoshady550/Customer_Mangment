using Customer_Mangment.CQRS.Customers.Queries.Report.Dtos;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Entities.History;
using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces.AppMediator;
using Customer_Mangment.Repository.Interfaces.Report;
using MongoDB.Driver;

namespace Customer_Mangment.CQRS.Customers.Queries.Report
{
    public sealed class GetCustomerReportQueryHandler(
        IMongoCollection<User> userRepo,
        IMongoCollection<CustomerSnapshot> customerRepo,
        ICustomerReportBuilder reportBuilder,
        ILogger<GetCustomerReportQueryHandler> logger)
        : IAppRequestHandler<GetCustomerReportQuery, Result<byte[]>>
    {
        private readonly IMongoCollection<User> _userRepo = userRepo;
        private readonly IMongoCollection<CustomerSnapshot> _customerRepo = customerRepo;
        private readonly ICustomerReportBuilder _reportBuilder = reportBuilder;
        private readonly ILogger<GetCustomerReportQueryHandler> _logger = logger;

        public async Task<Result<byte[]>> Handle(GetCustomerReportQuery request,
                                                 CancellationToken ct = default)
        {
            // 1️⃣ Check User
            var user = await _userRepo
                .Find(u => u.Id == request.UserId)
                .FirstOrDefaultAsync(ct);

            if (user is null)
            {
                _logger.LogWarning(
                    "Report requested by unknown user {UserId}", request.UserId);

                return Error.Unauthorized(
                    "UserNotFound",
                    $"User with ID {request.UserId} not found.");
            }

            // 2️⃣ Fetch Customers
            var customers = await _customerRepo
                .Find(c => c.ValidFrom >= request.From && c.ValidFrom < request.To)
                .ToListAsync(ct);

            // 3️⃣ Map To DTO
            var rows = customers
                .Select(c => new CustomerReportRow(
                    c.Id,
                    c.Name,
                    c.Mobile,
                    c.CreatedBy,
                    c.UpdatedBy,
                    c.IsDeleted))
                .ToList();

            if (rows.Count == 0)
            {
                _logger.LogInformation(
                    "Report query by user {UserId}: no customers found.",
                    request.UserId);

                return Error.NotFound(
                    "NoCustomersFound",
                    "No customers found.");
            }

            // 4️⃣ Build PDF
            _logger.LogInformation(
                "Building customer report PDF. Rows={Count}",
                rows.Count);

            var pdfBytes = _reportBuilder.Build(rows, request.From, request.To);

            _logger.LogInformation(
                "Customer report generated successfully. Size={Bytes}",
                pdfBytes.Length);

            return pdfBytes;
        }
    }
}