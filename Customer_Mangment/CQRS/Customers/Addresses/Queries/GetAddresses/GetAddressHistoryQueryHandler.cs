using Customer_Mangment.CQRS.Customers.DTOS;
using Customer_Mangment.CQRS.Customers.Queries.GetCustomers;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces;
using Customer_Mangment.Repository.Interfaces.AppMediator;
using Customer_Mangment.Repository.Interfaces.Audit;

namespace Customer_Mangment.CQRS.Customers.Addresses.Queries.GetAddresses
{
    public sealed class GetAddressHistoryQueryHandler(IGenericRepo<User> userRepo,
                                          IHistoryService historyService,
                                          ILogger<GetCustomerHistoryQueryHandler> logger) : IAppRequestHandler<GetAddressHistoryQuery, Result<List<AddressHistoryDto>>>
    {
        private readonly IGenericRepo<User> _userRepo = userRepo;
        private readonly IHistoryService _historyService = historyService;
        private readonly ILogger<GetCustomerHistoryQueryHandler> _logger = logger;
        public async Task<Result<List<AddressHistoryDto>>> Handle(GetAddressHistoryQuery request, CancellationToken ct)
        {
            var user = await _userRepo.AsNoTracking().FirstOrDefaultAsync(u => u.Id == request.UserId, ct);
            if (user == null)
            {
                _logger.LogWarning("Unauthorized access attempt by user with ID {UserId} to get address history for customer {CustomerId}. User not found.", request.UserId, request.CustomerId);
                return Error.Unauthorized("UserNotFound", $"User with ID {request.UserId} not found.");
            }

            var addressHistory = await _historyService.GetAddressHistoryAsync(request.CustomerId, ct);

            if (!addressHistory.Any())
            {
                _logger.LogInformation("No address history found for customer {CustomerId} when requested by user {UserId}.", request.CustomerId, request.UserId);
                return Error.NotFound("CustomerHistoryNotFound", $"No history found for customer {request.CustomerId}.");
            }
            return addressHistory;
        }
    }

}
