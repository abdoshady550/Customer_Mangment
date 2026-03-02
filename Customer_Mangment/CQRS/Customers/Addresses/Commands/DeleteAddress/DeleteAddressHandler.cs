using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces;
using Customer_Mangment.Repository.Interfaces.AppMediator;
using Customer_Mangment.Repository.Interfaces.Audit;

namespace Customer_Mangment.CQRS.Customers.Addresses.Commands.DeleteAddress
{
    public sealed class DeleteAddressHandler(IGenericRepo<Address> addressRepo,
                                             IGenericRepo<User> userRepo,
                                             IGenericRepo<Customer> customerRepo,
                                             ISnapshotService snapshotService,
                                             ILogger<DeleteAddressHandler> logger) : IAppRequestHandler<DeleteAddressCommand, Result<Deleted>>
    {
        private readonly IGenericRepo<Address> _addressRepo = addressRepo;
        private readonly IGenericRepo<User> _userRepo = userRepo;
        private readonly IGenericRepo<Customer> _customerRepo = customerRepo;
        private readonly ISnapshotService _snapshotService = snapshotService;
        private readonly ILogger<DeleteAddressHandler> _logger = logger;

        public async Task<Result<Deleted>> Handle(DeleteAddressCommand request, CancellationToken ct)
        {
            var user = await _userRepo.FirstOrDefaultAsync(u => u.Id == request.UserId, ct);
            if (user == null)
            {
                _logger.LogWarning("User with id {UserId} not found", request.UserId);
                return Error.NotFound("UserNotFound", $"User with id {request.UserId} not found");
            }
            var address = await _addressRepo.FirstOrDefaultAsync(a => a.Id == request.AddressId, ct);
            if (address == null)
            {
                _logger.LogWarning("Address with id {AddressId} not found", request.AddressId);
                return Error.NotFound("AddressNotFound", $"Address with id {request.AddressId} not found");
            }
            var customer = await _customerRepo.Include(x => x.Addresses).FirstOrDefaultAsync(c => c.Id == address.CustomerId, ct);
            if (customer == null)
            {
                _logger.LogWarning("Customer with id {CustomerId} not found", address.CustomerId);
                return Error.NotFound("CustomerNotFound", $"Customer with id {address.CustomerId} not found");
            }
            await snapshotService.SaveAddressSnapshotAsync(address, "Deleted", ct);

            _addressRepo.Remove(address);
            await _addressRepo.SaveChangesAsync(ct);
            return Result.Deleted;
        }
    }
}
