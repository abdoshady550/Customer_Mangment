using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces;
using Customer_Mangment.Repository.Interfaces.AppMediator;

namespace Customer_Mangment.CQRS.Customers.Commands.DeleteCustomer
{
    public sealed class DeleteCustomerHandler(IGenericRepo<User> userRepo,
                                              IGenericRepo<Customer> customerRepo,
                                              ILogger<DeleteCustomerHandler> logger) : IAppRequestHandler<DeleteCustomerCommand, Result<Deleted>>
    {
        private readonly IGenericRepo<User> _userRepo = userRepo;
        private readonly IGenericRepo<Customer> _customerRepo = customerRepo;
        private readonly ILogger<DeleteCustomerHandler> _logger = logger;

        public async Task<Result<Deleted>> Handle(DeleteCustomerCommand request, CancellationToken ct = default)
        {
            var user = await _userRepo.FirstOrDefaultAsync(u => u.Id == request.UserId, ct);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found.", request.UserId);
                return Error.NotFound("UserNotFound", $"User with ID {request.UserId} not found.");
            }
            var customer = await _customerRepo.Include(a => a.Addresses).FirstOrDefaultAsync(c => c.Id == request.CustomerId, ct);
            if (customer == null)
            {
                _logger.LogWarning("Customer with ID {CustomerId} not found.", request.CustomerId);
                return Error.NotFound("CustomerNotFound", $"Customer with ID {request.CustomerId} not found.");
            }
            customer.DeleteCustomer();
            _customerRepo.Update(customer);
            await _customerRepo.SaveChangesAsync(ct);
            _logger.LogInformation("Customer with ID {CustomerId} deleted by User with ID {UserId}.", request.CustomerId, request.UserId);
            return Result.Deleted;
        }
    };
}
