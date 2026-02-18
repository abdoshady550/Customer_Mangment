using AutoMapper;
using Customer_Mangment.CQRS.Customers.DTOS;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces;
using MediatR;

namespace Customer_Mangment.CQRS.Customers.Commands.CreateCustomer
{
    public sealed class CreateCustomerHandler(IGenericRepo<Customer> repo,
                                              IGenericRepo<CustomerHistory> auditRepo,
                                              IGenericRepo<User> userRepo,
                                              IMapper mapper,
                                              ILogger<CreateCustomerHandler> logger) : IRequestHandler<CreateCustomerCommand, Result<CustomerDto>>
    {
        private readonly IGenericRepo<Customer> _repo = repo;
        private readonly IGenericRepo<CustomerHistory> _auditRepo = auditRepo;
        private readonly IGenericRepo<User> _userRepo = userRepo;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<CreateCustomerHandler> _logger = logger;

        public async Task<Result<CustomerDto>> Handle(CreateCustomerCommand request, CancellationToken ct = default)
        {
            var user = await _userRepo.FindAsync(request.userId, ct);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found.", request.userId);
                return Error.NotFound("UserNotFound", $"User with ID {request.userId} not found.");
            }

            var mobileNumber = request.Mobile.Trim().ToLower();
            var existedCustomer = await _repo.AnyAsync(c => c.Mobile.ToLower() == mobileNumber, ct);

            if (existedCustomer)
            {
                _logger.LogWarning("Customer with mobile {Mobile} already exists.", request.Mobile);
                return Error.Conflict("CustomerAlreadyExists", $"Customer with mobile {request.Mobile} already exists.");
            }
            List<Address> addresses = new();
            foreach (var a in request.Adresses)
            {
                var addressResult = Address.CreateAddress(a.Type, a.Value);
                if (addressResult.IsError)
                {
                    _logger.LogWarning("Invalid address: {Errors}", string.Join(", ", addressResult.Errors.Select(e => e.Description)));
                    return addressResult.Errors;
                }
                var existedAddress = addresses.Any(ad => ad.Type == a.Type);
                if (existedAddress)
                {
                    _logger.LogWarning("Duplicate address type: {Type}", a.Type);
                    return Error.Validation("DuplicateAddressType", $"Duplicate address type: {a.Type}");
                }
                addresses.Add(addressResult.Value);
            }
            var customerResult = Customer.CreateCustomer(request.Name, request.Mobile, addresses);
            if (customerResult.IsError)
            {
                _logger.LogWarning("Failed to create customer: {Errors}", string.Join(", ", customerResult.Errors.Select(e => e.Description)));
                return customerResult.Errors;
            }

            var customer = customerResult.Value;
            await _repo.AddAsync(customer, ct);
            await _repo.SaveChangesAsync(ct);
            _logger.LogInformation("Customer with ID {CustomerId} created successfully.", customer.Id);

            var customerHistory = CustomerHistory.CreateCustomerHistory(customer.Id, customer.Name, customer.Mobile, user.UserName!);
            if (customerHistory.IsError)
            {
                _logger.LogWarning("Failed to create customer history: {Errors}", string.Join(", ", customerHistory.Errors.Select(e => e.Description)));
                return customerHistory.Errors;
            }
            await _auditRepo.AddAsync(customerHistory.Value, ct);
            await _auditRepo.SaveChangesAsync(ct);
            _logger.LogInformation("Customer history for customer ID {CustomerId} created successfully.", customer.Id);

            var customerDto = _mapper.Map<CustomerDto>(customer);

            return customerDto;
        }
    };
}
