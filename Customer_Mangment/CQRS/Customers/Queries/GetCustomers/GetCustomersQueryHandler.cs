using AutoMapper;
using Customer_Mangment.CQRS.Customers.DTOS;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces;
using MediatR;

namespace Customer_Mangment.CQRS.Customers.Queries.GetCustomers
{
    public sealed class GetCustomersQueryHandler(IGenericRepo<User> userRepo,
                                          IGenericRepo<Customer> customerRepo,
                                          IMapper mapper,
                                          ILogger<GetCustomersQueryHandler> logger) : IRequestHandler<GetCustomersQuery, Result<List<CustomerDto>>>
    {
        private readonly IGenericRepo<User> _userRepo = userRepo;
        private readonly IGenericRepo<Customer> _customerRepo = customerRepo;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<GetCustomersQueryHandler> _logger = logger;

        public async Task<Result<List<CustomerDto>>> Handle(GetCustomersQuery request, CancellationToken ct)
        {
            var user = await _userRepo.FirstOrDefaultAsync(u => u.Id == request.UserId, ct);
            if (user is null)
            {
                _logger.LogWarning("User with id {UserId} not found.", request.UserId);
                return Error.NotFound("UserNotFound", $"User with id {request.UserId} not found.");
            }

            if (request.CustomerId.HasValue)
            {
                List<CustomerDto> customers = new();

                var customer = await _customerRepo.AsNoTracking().Include(a => a.Addresses).FirstOrDefaultAsync(c => c.Id == request.CustomerId, ct);
                if (customer is null)
                {
                    _logger.LogWarning("Customer with id {CustomerId} not found.", request.CustomerId);
                    return Error.NotFound("CustomerNotFound", $"Customer with id {request.CustomerId} not found.");
                }
                var customerDto = _mapper.Map<Customer, CustomerDto>(customer);
                customers.Add(customerDto);
                return customers;
            }
            var allCustomers = await _customerRepo.AsNoTracking().Include(a => a.Addresses).ToListAsync(ct);

            var allCustomerDtos = _mapper.Map<List<Customer>, List<CustomerDto>>(allCustomers);

            _logger.LogInformation("Retrieved {CustomerCount} customers for user with id {UserId}.", allCustomerDtos.Count, request.UserId);

            return allCustomerDtos;
        }
    }
}
