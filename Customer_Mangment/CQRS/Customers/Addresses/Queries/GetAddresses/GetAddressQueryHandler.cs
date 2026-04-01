using Customer_Mangment.CQRS.Customers.Addresses.DTOS;
using Customer_Mangment.CQRS.Customers.Mappers;
using Customer_Mangment.CQRS.Customers.Queries.GetCustomers;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces;
using Customer_Mangment.Repository.Interfaces.AppMediator;
using Customer_Mangment.SharedResources;
using Customer_Mangment.SharedResources.Keys;
using Microsoft.Extensions.Localization;

namespace Customer_Mangment.CQRS.Customers.Addresses.Queries.GetAddresses
{
    public sealed class GetAddressQueryHandler(IGenericRepo<User> userRepo,
                                      IGenericRepo<Address> addressRepo,
                                      IGenericRepo<Customer> customerRepo,
                                       IStringLocalizer<SharedResource> localizer,
                                      ICustomerMapper mapper,
                                      ILogger<GetAddressQueryHandler> logger) : IAppRequestHandler<GetAddressQuery, Result<List<AddressDto>>>
    {
        private readonly IGenericRepo<User> _userRepo = userRepo;
        private readonly IGenericRepo<Address> _addressRepo = addressRepo;
        private readonly IGenericRepo<Customer> _customerRepo = customerRepo;
        private readonly IStringLocalizer<SharedResource> _localizer = localizer;
        private readonly ICustomerMapper _mapper = mapper;
        private readonly ILogger<GetAddressQueryHandler> _logger = logger;
        public async Task<Result<List<AddressDto>>> Handle(GetAddressQuery request, CancellationToken ct = default)
        {
            var user = await _userRepo.FirstOrDefaultAsync(u => u.Id == request.UserId, ct);
            if (user == null)
            {
                _logger.LogWarning("Unauthorized access attempt by user with ID {UserId} to get address history for customer {CustomerId}. User not found.", request.UserId, request.CustomerId);
                return LocalizedError.Unauthorized(_localizer, "UserNotFound", ResourceKeys.User.NotFound, request.UserId);
            }
            if (request.CustomerId.HasValue && !request.AddressId.HasValue)
            {
                var customer = await _customerRepo.AsNoTracking().FirstOrDefaultAsync(c => c.Id == request.CustomerId, ct);
                if (customer == null)
                {
                    _logger.LogInformation("Customer with ID {CustomerId} not found when requested by user {UserId}.", request.CustomerId, request.UserId);
                    return LocalizedError.NotFound(_localizer, "CustomerNotFound", ResourceKeys.Customer.NotFound, request.CustomerId);
                }
                var addresses = await _addressRepo.AsNoTracking().Where(a => a.CustomerId == request.CustomerId).ToListAsync(ct);
                _logger.LogInformation("Retrieved {AddressCount} addresses for customer {CustomerId} requested by user {UserId}.", addresses.Count, request.CustomerId, request.UserId);

                var dto = _mapper.ToAddressDtoList(addresses);
                return dto;

            }
            else if (request.AddressId.HasValue && request.CustomerId.HasValue)
            {
                var addresses = await _addressRepo.AsNoTracking().Where(a => a.CustomerId == request.CustomerId && a.Id == request.AddressId).ToListAsync(ct);
                if (!addresses.Any())
                {
                    _logger.LogInformation("Address with ID {AddressId} not found for customer {CustomerId} when requested by user {UserId}.", request.AddressId, request.CustomerId, request.UserId);
                    return Error.NotFound("AddressNotFound", $"Address with ID {request.AddressId} not found for customer {request.CustomerId}.");
                }
                var dto = _mapper.ToAddressDtoList(addresses);
                return dto;
            }
            else if (!request.CustomerId.HasValue && request.AddressId.HasValue)
            {
                var addresses = await _addressRepo.AsNoTracking().Where(a => a.Id == request.AddressId).ToListAsync(ct);
                _logger.LogInformation("Retrieved {AddressCount} addresses  requested by user {UserId}.", addresses.Count, request.UserId);

                var dto = _mapper.ToAddressDtoList(addresses);
                return dto;
            }
            else
            {
                var addresses = await _addressRepo.AsNoTracking().ToListAsync(ct);
                _logger.LogInformation("Retrieved all addresses  requested by user {UserId}.", request.UserId);
                var dto = _mapper.ToAddressDtoList(addresses);
                return dto;
            }
        }
    }

}
