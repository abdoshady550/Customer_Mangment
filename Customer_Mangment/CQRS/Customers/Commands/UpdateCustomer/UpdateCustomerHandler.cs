using AutoMapper;
using Customer_Mangment.CQRS.Customers.DTOS;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces;
using MediatR;
using System.Text.Json;

namespace Customer_Mangment.CQRS.Customers.Commands.UpdateCustomer
{
    public sealed class UpdateCustomerHandler(IGenericRepo<User> userRepo,
                                              IGenericRepo<Customer> customerRepo,
                                              IGenericRepo<CustomerHistory> historyRepo,
                                              IMapper mapper,
                                              ILogger<UpdateCustomerHandler> logger) : IRequestHandler<UpdateCustomerCommand, Result<Updated>>
    {
        private readonly IGenericRepo<User> _userRepo = userRepo;
        private readonly IGenericRepo<Customer> _customerRepo = customerRepo;
        private readonly IGenericRepo<CustomerHistory> _historyRepo = historyRepo;
        private readonly IMapper _mapper = mapper;
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
            var oldCustomerDto = _mapper.Map<CustomerDto>(customer);
            var oldCustomerData = JsonSerializer.Serialize(oldCustomerDto);
            var updateResult = customer.UpdateCustomer(request.Name, request.Mobile);
            if (updateResult.IsError)
            {
                _logger.LogWarning("Validation failed for updating customer with ID {CustomerId}. Errors: {Errors}", request.CustomerId, updateResult.Errors);
                return updateResult.Errors;
            }
            _customerRepo.Update(customer);
            var newCustomerDto = _mapper.Map<CustomerDto>(customer);

            var firstHistoryEntry = await _historyRepo.FirstOrDefaultAsync(h => h.CustomerId == customer.Id, ct);

            var historyEntry = CustomerHistory.UpdateCustomerHistory(customer.Id,
                                                                     user.UserName!,
                                                                     "Update customer data",
                                                                     firstHistoryEntry.CreatedAt,
                                                                     firstHistoryEntry.CreatedBy,
                                                                     oldCustomerData,
                                                                     JsonSerializer.Serialize(newCustomerDto));
            if (historyEntry.IsError)
            {
                _logger.LogError("Failed to create customer history for customer with ID {CustomerId}. Errors: {Errors}", request.CustomerId, historyEntry.Errors);
                return historyEntry.Errors;
            }
            await _historyRepo.AddAsync(historyEntry.Value, ct);

            await _customerRepo.SaveChangesAsync(ct);

            _logger.LogInformation("Customer with ID {CustomerId} updated successfully.", request.CustomerId);

            return Result.Updated;
        }
    };
}
