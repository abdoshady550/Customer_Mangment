using Customer_Mangment.CQRS.Customers.Addresses.DTOS;
using Customer_Mangment.CQRS.Customers.DTOS;
using Customer_Mangment.CQRS.Customers.Mappers;
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
            var user = await _userRepo.FirstOrDefaultAsync(u => u.Id == request.UserId, ct);
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

    public sealed class GetAddressQueryHandler(IGenericRepo<User> userRepo,
                                      IGenericRepo<Address> addressRepo,
                                      IGenericRepo<Customer> customerRepo,
                                      ICustomerMapper mapper,
                                      ILogger<GetAddressQueryHandler> logger) : IAppRequestHandler<GetAddressQuery, Result<List<AddressDto>>>
    {
        private readonly IGenericRepo<User> _userRepo = userRepo;
        private readonly IGenericRepo<Address> _addressRepo = addressRepo;
        private readonly IGenericRepo<Customer> _customerRepo = customerRepo;
        private readonly ICustomerMapper _mapper = mapper;
        private readonly ILogger<GetAddressQueryHandler> _logger = logger;
        public async Task<Result<List<AddressDto>>> Handle(GetAddressQuery request, CancellationToken ct = default)
        {
            var user = await _userRepo.FirstOrDefaultAsync(u => u.Id == request.UserId, ct);
            if (user == null)
            {
                _logger.LogWarning("Unauthorized access attempt by user with ID {UserId} to get address history for customer {CustomerId}. User not found.", request.UserId, request.CustomerId);
                return Error.Unauthorized("UserNotFound", $"User with ID {request.UserId} not found.");
            }
            var customer = await _customerRepo.FirstOrDefaultAsync(c => c.Id == request.CustomerId, ct);
            if (customer == null)
            {
                _logger.LogInformation("Customer with ID {CustomerId} not found when requested by user {UserId}.", request.CustomerId, request.UserId);
                return Error.NotFound("CustomerNotFound", $"Customer with ID {request.CustomerId} not found.");
            }

            if (request.AddressId.HasValue)
            {
                var addresses = await _addressRepo.Where(a => a.CustomerId == request.CustomerId && a.Id == request.AddressId).ToListAsync(ct);
                var dto = _mapper.ToAddressDtoList(addresses);

                return dto;
            }
            else
            {
                var addresses = await _addressRepo.Where(a => a.CustomerId == request.CustomerId).ToListAsync(ct);
                _logger.LogInformation("Retrieved {AddressCount} addresses for customer {CustomerId} requested by user {UserId}.", addresses.Count, request.CustomerId, request.UserId);

                var dto = _mapper.ToAddressDtoList(addresses);

                return dto;
            }
        }
    }

}
