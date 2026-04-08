using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Events;
using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces;
using Customer_Mangment.Repository.Interfaces.AppMediator;
using Customer_Mangment.SharedResources;
using Customer_Mangment.SharedResources.Keys;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using Wolverine;

namespace Customer_Mangment.CQRS.Customers.Addresses.Commands.DeleteAddress
{
    public sealed class DeleteAddressHandler(IGenericRepo<Address> addressRepo,
                                             IGenericRepo<User> userRepo,
                                             IGenericRepo<Customer> customerRepo,
                                             ISyncGenericRepo<Address> syncRepo,
                                             IMessageBus bus,
                                             IDistributedCache cache,
                                             IStringLocalizer<SharedResource> localizer,
                                             ILogger<DeleteAddressHandler> logger) : IAppRequestHandler<DeleteAddressCommand, Result<Deleted>>
    {
        private readonly IGenericRepo<Address> _addressRepo = addressRepo;
        private readonly IGenericRepo<User> _userRepo = userRepo;
        private readonly IGenericRepo<Customer> _customerRepo = customerRepo;
        private readonly ISyncGenericRepo<Address> _syncRepo = syncRepo;
        private readonly IDistributedCache _cache = cache;
        private readonly IMessageBus _bus = bus;
        private readonly IStringLocalizer<SharedResource> _localizer = localizer;
        private readonly ILogger<DeleteAddressHandler> _logger = logger;

        public async Task<Result<Deleted>> Handle(DeleteAddressCommand request, CancellationToken ct)
        {
            var user = await _userRepo.FirstOrDefaultAsync(u => u.Id == request.UserId, ct);
            if (user == null)
            {
                _logger.LogWarning("User with id {UserId} not found", request.UserId);
                return LocalizedError.Unauthorized(_localizer, "UserNotFound", ResourceKeys.User.NotFound, request.UserId);
            }
            var address = await _addressRepo.FirstOrDefaultAsync(a => a.Id == request.AddressId, ct);
            if (address == null)
            {
                _logger.LogWarning("Address with id {AddressId} not found", request.AddressId);
                return LocalizedError.NotFound(_localizer, "AddressNotFound", ResourceKeys.Address.NotFound, request.AddressId);

            }
            var customer = await _customerRepo.Include(x => x.Addresses).FirstOrDefaultAsync(c => c.Id == address.CustomerId, ct);
            if (customer == null)
            {
                _logger.LogWarning("Customer with id {CustomerId} not found", address.CustomerId);
                return LocalizedError.NotFound(_localizer, "CustomerNotFound", ResourceKeys.Customer.NotFound, address.CustomerId);
            }
            await _bus.PublishAsync(new AddressDeletedEvent(address));

            _addressRepo.Remove(address);
            await _addressRepo.SaveChangesAsync(ct);
            //_syncRepo.Remove(address);
            //await _syncRepo.SaveChangesAsync(ct);
            await _cache.RemoveAsync("GetAllAddresses", ct);


            return Result.Deleted;
        }
    }
}
