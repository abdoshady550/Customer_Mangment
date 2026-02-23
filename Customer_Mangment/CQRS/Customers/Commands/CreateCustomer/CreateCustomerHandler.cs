using AutoMapper;
using Customer_Mangment.CQRS.Customers.DTOS;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces;
using Customer_Mangment.Repository.Interfaces.AppMediator;

namespace Customer_Mangment.CQRS.Customers.Commands.CreateCustomer
{
    public sealed class CreateCustomerHandler(IGenericRepo<Customer> repo,
                                              IGenericRepo<User> userRepo,
                                              IMapper mapper,
                                              ILogger<CreateCustomerHandler> logger) : IAppRequestHandler<CreateCustomerCommand, Result<CustomerDto>>
    {
        private readonly IGenericRepo<Customer> _repo = repo;
        private readonly IGenericRepo<User> _userRepo = userRepo;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<CreateCustomerHandler> _logger = logger;

        public async Task<Result<CustomerDto>> Handle(CreateCustomerCommand request, CancellationToken ct = default)
        {
            var user = await _userRepo.FirstOrDefaultAsync(u => u.Id == request.UserId, ct);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found.", request.UserId);
                return Error.NotFound("UserNotFound", $"User with ID {request.UserId} not found.");
            }

            var mobileNumber = request.Mobile.Trim().ToLower();
            var existedCustomer = await _repo.AnyAsync(c => c.Mobile.ToLower() == mobileNumber, ct);

            if (existedCustomer)
            {
                _logger.LogWarning("Customer with mobile {Mobile} already exists.", request.Mobile);
                return Error.Conflict("CustomerAlreadyExists", $"Customer with mobile {request.Mobile} already exists.");
            }
            var customerResult = Customer.CreateCustomer(request.Name, request.Mobile, user.UserName!, request.Adresses.Select(a => (a.Type, a.Value)));
            if (customerResult.IsError)
            {
                _logger.LogWarning("Failed to create customer: {Errors}", string.Join(", ", customerResult.Errors.Select(e => e.Description)));
                return customerResult.Errors;
            }

            var customer = customerResult.Value;
            await _repo.AddAsync(customer, ct);
            await _repo.SaveChangesAsync(ct);

            _logger.LogInformation("Customer with ID {CustomerId} created successfully.", customer.Id);

            var customerDto = _mapper.Map<CustomerDto>(customer);

            return customerDto;
        }
    };
}
