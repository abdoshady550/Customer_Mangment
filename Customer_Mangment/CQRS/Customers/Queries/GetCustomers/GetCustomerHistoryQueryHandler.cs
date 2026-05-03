using Customer_Mangment.CQRS.Customers.DTOS;
using Customer_Mangment.Repository.Interfaces;
using Customer_Mangment.Repository.Interfaces.AppMediator;
using Customer_Mangment.Repository.Interfaces.Audit;
using Customer_Mangment.SharedResources;
using Customer_Mangment.SharedResources.Keys;
using Microsoft.Extensions.Localization;

namespace Customer_Mangment.CQRS.Customers.Queries.GetCustomers
{
    public sealed class GetCustomerHistoryQueryHandler(
                                          IHistoryService historyService,
                                           IStringLocalizer<SharedResource> localizer,
                                           IIdentityService identityService,
                                          ILogger<GetCustomerHistoryQueryHandler> logger) : IAppRequestHandler<GetCustomerHistoryQuery, Model.Results.Result<List<CustomerHistoryDto>>>
    {
        private readonly IHistoryService _historyService = historyService;
        private readonly IStringLocalizer<SharedResource> _localizer = localizer;
        private readonly ILogger<GetCustomerHistoryQueryHandler> _logger = logger;
        private readonly IIdentityService _identityService = identityService;

        public async Task<Model.Results.Result<List<CustomerHistoryDto>>> Handle(GetCustomerHistoryQuery request, CancellationToken ct)
        {
            var user = await _identityService.GetUserByIdAsync(request.UserId);
            if (user.IsError)
            {
                _logger.LogWarning("User with id {UserId} not found.", request.UserId);
                return LocalizedError.Unauthorized(_localizer, "UserNotFound", ResourceKeys.User.NotFound, request.UserId);
            }

            var customerHistory = await _historyService.GetCustomerHistoryAsync(request.CustomerId, ct);

            if (!customerHistory.Any())
            {
                _logger.LogWarning("No history found for customer with id {CustomerId}.", request.CustomerId);
                return LocalizedError.NotFound(_localizer, "CustomerHistoryNotFound", ResourceKeys.Customer.HistoryNotFound, request.CustomerId);

            }
            return customerHistory;
        }
    }
}
