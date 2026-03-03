using Customer_Mangment.CQRS.Customers.DTOS;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces;
using Customer_Mangment.Repository.Interfaces.AppMediator;
using Customer_Mangment.Repository.Interfaces.Audit;

namespace Customer_Mangment.CQRS.Customers.Queries.GetCustomers
{
    public sealed class GetCustomerHistoryQueryHandler(IGenericRepo<User> userRepo,
                                          IHistoryService historyService,
                                          ILogger<GetCustomerHistoryQueryHandler> logger) : IAppRequestHandler<GetCustomerHistoryQuery, Result<List<CustomerHistoryDto>>>
    {
        private readonly IGenericRepo<User> _userRepo = userRepo;
        private readonly IHistoryService _historyService = historyService;
        private readonly ILogger<GetCustomerHistoryQueryHandler> _logger = logger;
        public async Task<Result<List<CustomerHistoryDto>>> Handle(GetCustomerHistoryQuery request, CancellationToken ct)
        {
            var user = await _userRepo.AsNoTracking().FirstOrDefaultAsync(u => u.Id == request.UserId, ct);
            if (user == null)
            {
                _logger.LogWarning("User with id {UserId} not found.", request.UserId);
                return Error.NotFound("UserNotFound", $"User with id {request.UserId} not found.");
            }

            var customerHistory = await _historyService.GetCustomerHistoryAsync(request.CustomerId, ct);

            if (!customerHistory.Any())
            {
                _logger.LogWarning("No history found for customer with id {CustomerId}.", request.CustomerId);
                return Error.NotFound("CustomerHistoryNotFound", $"No history found for customer {request.CustomerId}.");

            }
            return customerHistory;
        }
    }
}
