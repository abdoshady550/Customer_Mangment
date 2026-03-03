using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Events;
using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces;
using Customer_Mangment.Repository.Interfaces.AppMediator;
using Wolverine;

namespace Customer_Mangment.CQRS.Customers.Commands.UpdateCustomer
{
    public sealed class UpdateCustomerHandler(IGenericRepo<User> userRepo,
                                              IGenericRepo<Customer> customerRepo,
                                              IMessageBus bus,
                                              ILogger<UpdateCustomerHandler> logger) : IAppRequestHandler<UpdateCustomerCommand, Result<Updated>>
    {
        private readonly IGenericRepo<User> _userRepo = userRepo;
        private readonly IGenericRepo<Customer> _customerRepo = customerRepo;
        private readonly IMessageBus _bus = bus;
        private readonly ILogger<UpdateCustomerHandler> _logger = logger;
        public async Task<Result<Updated>> Handle(UpdateCustomerCommand request, CancellationToken ct = default)
        {
            var user = await _userRepo.FirstOrDefaultAsync(u => u.Id == request.UserId, ct);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found.", request.UserId);
                return Error.NotFound("UserNotFound", $"User with ID {request.UserId} not found.");
            }
            var customer = await _customerRepo.Include(c => c.Addresses).FirstOrDefaultAsync(c => c.Id == request.CustomerId, ct);
            if (customer == null)
            {
                _logger.LogWarning("Customer with ID {CustomerId} not found.", request.CustomerId);
                return Error.NotFound("CustomerNotFound", $"Customer with ID {request.CustomerId} not found.");
            }

            var updateResult = customer.UpdateCustomer(request.Name, request.Mobile, user.UserName!);
            if (updateResult.IsError)
            {
                _logger.LogWarning("Validation failed for updating customer with ID {CustomerId}. Errors: {Errors}", request.CustomerId, updateResult.Errors);
                return updateResult.Errors;
            }
            _customerRepo.Update(customer);


            await _customerRepo.SaveChangesAsync(ct);

            await _bus.PublishAsync(new CustomerUpdatedEvent(customer));

            _logger.LogInformation("Customer with ID {CustomerId} updated successfully.", request.CustomerId);

            return Result.Updated;
        }
    };
}
