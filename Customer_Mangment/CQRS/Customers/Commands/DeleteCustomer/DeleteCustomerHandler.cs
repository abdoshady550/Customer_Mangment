using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Events;
using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces;
using Customer_Mangment.Repository.Interfaces.AppMediator;
using Customer_Mangment.SharedResources;
using Customer_Mangment.SharedResources.Keys;
using Microsoft.Extensions.Localization;
using Wolverine;

namespace Customer_Mangment.CQRS.Customers.Commands.DeleteCustomer
{
    public sealed class DeleteCustomerHandler(IGenericRepo<User> userRepo,
                                              IGenericRepo<Customer> customerRepo,
                                              ISyncGenericRepo<Customer> syncRepo,
                                               IStringLocalizer<SharedResource> localizer,
                                              IMessageBus bus,
                                              ILogger<DeleteCustomerHandler> logger) : IAppRequestHandler<DeleteCustomerCommand, Result<Deleted>>
    {
        private readonly IGenericRepo<User> _userRepo = userRepo;
        private readonly IGenericRepo<Customer> _customerRepo = customerRepo;
        private readonly ISyncGenericRepo<Customer> _syncRepo = syncRepo;
        private readonly IStringLocalizer<SharedResource> _localizer = localizer;
        private readonly IMessageBus _bus = bus;
        private readonly ILogger<DeleteCustomerHandler> _logger = logger;

        public async Task<Result<Deleted>> Handle(DeleteCustomerCommand request, CancellationToken ct = default)
        {
            var user = await _userRepo.FirstOrDefaultAsync(u => u.Id == request.UserId, ct);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found.", request.UserId);
                return LocalizedError.Unauthorized(_localizer, "UserNotFound", ResourceKeys.User.NotFound, request.UserId);
            }
            var customer = await _customerRepo.Include(a => a.Addresses).FirstOrDefaultAsync(c => c.Id == request.CustomerId, ct);
            if (customer == null)
            {
                _logger.LogWarning("Customer with ID {CustomerId} not found.", request.CustomerId);
                return LocalizedError.NotFound(_localizer, "CustomerNotFound", ResourceKeys.Customer.NotFound, request.CustomerId);
            }
            customer.DeleteCustomer();

            _customerRepo.Update(customer);
            await _customerRepo.SaveChangesAsync(ct);

            //_syncRepo.Update(customer);
            //await _syncRepo.SaveChangesAsync(ct);


            await _bus.PublishAsync(new CustomerDeletedEvent(customer));

            _logger.LogInformation("Customer with ID {CustomerId} deleted by User with ID {UserId}.", request.CustomerId, request.UserId);
            return Result.Deleted;
        }
    };
}
