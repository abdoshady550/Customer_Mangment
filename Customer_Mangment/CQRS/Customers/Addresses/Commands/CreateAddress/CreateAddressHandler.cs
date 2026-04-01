using Customer_Mangment.CQRS.Customers.Addresses.DTOS;
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

namespace Customer_Mangment.CQRS.Customers.Addresses.Commands.CreateAddress
{
    public class CreateAddressHandler(IGenericRepo<User> userRepo,
                                             IGenericRepo<Customer> customerRepo,
                                             IGenericRepo<Address> adressRepo,
                                             ISyncGenericRepo<Address> SyncadressRepo,
                                             IStringLocalizer<SharedResource> localizer,
                                             IMessageBus bus,
                                             ICustomerMapper mapper,
                                             ILogger<CreateAddressHandler> logger) : IAppRequestHandler<AddAddressCommand, Result<AddressDto>>

    {
        private readonly IGenericRepo<User> _userRepo = userRepo;
        private readonly IGenericRepo<Customer> _customerRepo = customerRepo;
        private readonly IGenericRepo<Address> _adressRepo = adressRepo;
        private readonly ISyncGenericRepo<Address> _syncadressRepo = SyncadressRepo;
        private readonly IStringLocalizer<SharedResource> _localizer = localizer;
        private readonly IMessageBus _bus = bus;
        private readonly ICustomerMapper _mapper = mapper;
        private readonly ILogger<CreateAddressHandler> _logger = logger;

        public async Task<Result<AddressDto>> Handle(AddAddressCommand request, CancellationToken ct)
        {
            var user = await _userRepo.FirstOrDefaultAsync(u => u.Id == request.UserId, ct);

            if (user == null)
            {
                _logger.LogWarning("User with id {UserId} not found", request.UserId);
                return LocalizedError.Unauthorized(_localizer, "UserNotFound", ResourceKeys.User.NotFound, request.UserId);
            }
            var customer = await _customerRepo.Include(c => c.Addresses).FirstOrDefaultAsync(c => c.Id == request.CustomerId, ct);
            if (customer == null)
            {
                _logger.LogWarning("Customer with id {CustomerId} not found", request.CustomerId);
                return LocalizedError.NotFound(_localizer, "CustomerNotFound", ResourceKeys.Customer.NotFound, request.CustomerId);
            }
            var address = customer.AddAddress(request.Type, request.Value, _localizer);
            if (address.IsError)
            {
                _logger.LogWarning("Failed to add address to customer with id {CustomerId}. Error: {Error}", request.CustomerId, address.Errors);
                return address.Errors;
            }
            var repeatedAddress = await _adressRepo
                .AnyAsync(a => a.CustomerId == address.Value.CustomerId && a.Type == request.Type && a.Id != address.Value.Id, ct);
            if (repeatedAddress)
            {
                _logger.LogWarning("Address of type {AddressType} already exists for customer with id {CustomerId}", request.Type, address.Value.CustomerId);
                return LocalizedError.Conflict(_localizer, "DuplicateAddress", ResourceKeys.Address.Duplicate, address.Value.CustomerId);

            }


            await _adressRepo.AddAsync(address.Value, ct);
            _customerRepo.Update(customer);
            await _customerRepo.SaveChangesAsync(ct);

            //await _syncadressRepo.AddAsync(address.Value, ct);
            //await _syncadressRepo.SaveChangesAsync(ct);

            await _bus.PublishAsync(new AddressCreatedEvent(address.Value));


            var addressDto = _mapper.ToAddressDto(address.Value);

            return addressDto;

        }
    }
}
