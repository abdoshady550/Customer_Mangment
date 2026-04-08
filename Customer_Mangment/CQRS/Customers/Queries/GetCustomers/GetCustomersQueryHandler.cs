using Customer_Mangment.CQRS.Customers.DTOS;
using Customer_Mangment.CQRS.Customers.Mappers;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces;
using Customer_Mangment.Repository.Interfaces.AppMediator;
using Customer_Mangment.Repository.Interfaces.tenantCache;
using Customer_Mangment.SharedResources;
using Customer_Mangment.SharedResources.Keys;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Customer_Mangment.CQRS.Customers.Queries.GetCustomers
{
    public sealed class GetCustomersQueryHandler(IGenericRepo<User> userRepo,
                                          IGenericRepo<Customer> customerRepo,
                                          ICustomerMapper mapper,
                                          ITenantCachedQueryService cacheService,
                                          IStringLocalizer<SharedResource> localizer,
                                          ILogger<GetCustomersQueryHandler> logger) : IAppRequestHandler<GetCustomersQuery, Result<List<CustomerDto>>>
    {
        private readonly IGenericRepo<User> _userRepo = userRepo;
        private readonly IGenericRepo<Customer> _customerRepo = customerRepo;
        private readonly ICustomerMapper _mapper = mapper;
        private readonly ITenantCachedQueryService _cacheService = cacheService;
        private readonly IStringLocalizer<SharedResource> _localizer = localizer;
        private readonly ILogger<GetCustomersQueryHandler> _logger = logger;

        public async Task<Result<List<CustomerDto>>> Handle(GetCustomersQuery request, CancellationToken ct)
        {
            var user = await _userRepo.AsNoTracking().FirstOrDefaultAsync(u => u.Id == request.UserId, ct);
            if (user is null)
            {
                _logger.LogWarning("User with id {UserId} not found.", request.UserId);
                return LocalizedError.Unauthorized(_localizer, "UserNotFound", ResourceKeys.User.NotFound, request.UserId);
            }

            if (request.CustomerId.HasValue)
            {
                List<CustomerDto> customers = new();

                var customer = await _customerRepo.AsNoTracking().Include(a => a.Addresses).FirstOrDefaultAsync(c => c.Id == request.CustomerId, ct);
                if (customer is null)
                {
                    _logger.LogWarning("Customer with id {CustomerId} not found.", request.CustomerId);
                    return LocalizedError.NotFound(_localizer, "CustomerNotFound", ResourceKeys.Customer.NotFound, request.CustomerId);

                }
                var customerDto = _mapper.ToCustomerDto(customer);
                customers.Add(customerDto);
                return customers;
            }

            var customersCached = await _cacheService.GetOrCreateAsync("GetCustomers", async (dbContext, token) =>
            {
                logger.LogInformation("Loading customers from DB for tenant");
                var allCustomers = await dbContext.Customers
                    .Include(c => c.Addresses)
                    .ToListAsync(token);
                return _mapper.ToCustomerDtoList(allCustomers);
            }, ct);
            if (!_cacheService.LoadedFromDb)
            {
                _logger.LogInformation("GetCustomers From cache");
            }
            return customersCached ?? [];
        }
    }
}
