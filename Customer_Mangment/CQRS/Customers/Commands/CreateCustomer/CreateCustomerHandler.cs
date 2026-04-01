using Customer_Mangment.CQRS.Customers.Addresses.Commands.CreateAddress;
using Customer_Mangment.CQRS.Customers.Addresses.DTOS;
using Customer_Mangment.CQRS.Customers.DTOS;
using Customer_Mangment.CQRS.Customers.Mappers;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Events;
using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces;
using Customer_Mangment.Repository.Interfaces.AppMediator;
using Customer_Mangment.SharedResources;
using Customer_Mangment.SharedResources.Keys;
using Microsoft.Extensions.Localization;
using Wolverine;

namespace Customer_Mangment.CQRS.Customers.Commands.CreateCustomer
{
    public sealed class CreateCustomerHandler(IGenericRepo<Customer> repo,
                                              IGenericRepo<User> userRepo,
                                              ISyncGenericRepo<Customer> syncRepo,
                                              IMessageBus bus,
                                              ICustomerMapper mapper,
                                               IStringLocalizer<SharedResource> localizer,
                                              ILogger<CreateCustomerHandler> logger) : IAppRequestHandler<CreateCustomerCommand, Result<CustomerDto>>
    {
        private readonly IGenericRepo<Customer> _repo = repo;
        private readonly IGenericRepo<User> _userRepo = userRepo;
        private readonly ISyncGenericRepo<Customer> _syncRepo = syncRepo;
        private readonly IMessageBus _bus = bus;
        private readonly ICustomerMapper _mapper = mapper;
        private readonly IStringLocalizer<SharedResource> _localizer = localizer;
        private readonly ILogger<CreateCustomerHandler> _logger = logger;

        public async Task<Result<CustomerDto>> Handle(CreateCustomerCommand request, CancellationToken ct = default)
        {
            var user = await _userRepo.FirstOrDefaultAsync(u => u.Id == request.UserId, ct);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found.", request.UserId);
                return LocalizedError.Unauthorized(_localizer, "UserNotFound", ResourceKeys.User.NotFound, request.UserId);
            }

            var mobileNumber = request.Mobile.Trim().ToLower();
            var existedCustomer = await _repo.AnyAsync(c => c.Mobile.ToLower() == mobileNumber, ct);

            if (existedCustomer)
            {
                _logger.LogWarning("Customer with mobile {Mobile} already exists.", request.Mobile);
                return LocalizedError.Conflict(_localizer, "CustomerAlreadyExists", ResourceKeys.Customer.AlreadyExists, request.Mobile);


            }
            var customerResult = Customer.CreateCustomer(request.Name, request.Mobile, user.UserName!, [], _localizer);
            if (customerResult.IsError)
            {
                _logger.LogWarning("Failed to create customer: {Errors}", string.Join(", ", customerResult.Errors.Select(e => e.Description)));
                return customerResult.Errors;
            }

            var customer = customerResult.Value;
            await _repo.AddAsync(customer, ct);
            await _repo.SaveChangesAsync(ct);

            //await _syncRepo.AddAsync(customer, ct);
            //await _syncRepo.SaveChangesAsync(ct);

            foreach (var address in request.Adresses)
            {
                var addAddressResult = await _bus.InvokeAsync<Result<AddressDto>>(
                    new AddAddressCommand(request.UserId, customer.Id, address.Type, address.Value), ct);

                if (addAddressResult.IsError)
                {
                    _logger.LogWarning("Failed to add address to customer {CustomerId}: {Errors}", customer.Id, addAddressResult.Errors);
                    return addAddressResult.Errors;
                }
            }

            await _bus.PublishAsync(new CustomerCreatedEvent(customer));

            _logger.LogInformation("Customer with ID {CustomerId} created successfully.", customer.Id);

            var customerWithAddresses = await _repo.Include(c => c.Addresses)
                                       .FirstOrDefaultAsync(c => c.Id == customer.Id, ct);


            var customerDto = _mapper.ToCustomerDto(customerWithAddresses!);

            return customerDto;
        }

    };
}
