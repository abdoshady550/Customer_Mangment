using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces;
using MediatR;
using System.Text.Json;

namespace Customer_Mangment.CQRS.Customers.Commands.DeleteCustomer
{
    public sealed class DeleteCustomerHandler(IGenericRepo<User> userRepo,
                                              IGenericRepo<Customer> customerRepo,
                                              IGenericRepo<CustomerHistory> historyRepo,
                                              ILogger<DeleteCustomerHandler> logger) : IRequestHandler<DeleteCustomerCommand, Result<Deleted>>
    {
        private readonly IGenericRepo<User> _userRepo = userRepo;
        private readonly IGenericRepo<Customer> _customerRepo = customerRepo;
        private readonly IGenericRepo<CustomerHistory> _historyRepo = historyRepo;
        private readonly ILogger<DeleteCustomerHandler> _logger = logger;

        public async Task<Result<Deleted>> Handle(DeleteCustomerCommand request, CancellationToken ct = default)
        {
            var user = await _userRepo.FindAsync(request.UserId, ct);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found.", request.UserId);
                return Error.NotFound("UserNotFound", $"User with ID {request.UserId} not found.");
            }
            var customer = await _customerRepo.FirstOrDefaultAsync(c => c.Id == request.CustomerId, ct);
            if (customer == null)
            {
                _logger.LogWarning("Customer with ID {CustomerId} not found.", request.CustomerId);
                return Error.NotFound("CustomerNotFound", $"Customer with ID {request.CustomerId} not found.");
            }
            _customerRepo.Remove(customer);
            _logger.LogInformation("Customer with ID {CustomerId} deleted by User with ID {UserId}.", request.CustomerId, request.UserId);

            var historyEntry = CustomerHistory.UpdateCustomerHistory(
                customer.Id,
                user.UserName!,
                action: "Deleted",
                oldCustomer: JsonSerializer.Serialize(customer),
                newCustomer: "N/A"
            );
            if (historyEntry.IsError)
            {
                _logger.LogError("Failed to create customer history for Customer ID {CustomerId}. Error: {Error}", customer.Id, historyEntry.TopError);
                return historyEntry.Errors;
            }
            await _historyRepo.AddAsync(historyEntry.Value, ct);
            await _historyRepo.SaveChangesAsync(ct);
            _logger.LogInformation("Customer history entry created for Customer ID {CustomerId}.", customer.Id);

            return Result.Deleted;
        }
    };
}
