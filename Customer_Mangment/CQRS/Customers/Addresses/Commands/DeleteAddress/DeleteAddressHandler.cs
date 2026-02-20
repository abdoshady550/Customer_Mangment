using AutoMapper;
using Customer_Mangment.CQRS.Customers.DTOS;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces;
using MediatR;
using System.Text.Json;

namespace Customer_Mangment.CQRS.Customers.Addresses.Commands.DeleteAddress
{
    public sealed class DeleteAddressHandler(IGenericRepo<Address> addressRepo,
                                             IGenericRepo<User> userRepo,
                                             IGenericRepo<Customer> customerRepo,
                                             IGenericRepo<CustomerHistory> auditRepo, IMapper mapper, ILogger<DeleteAddressHandler> logger) : IRequestHandler<DeleteAddressCommand, Result<Deleted>>
    {
        private readonly IGenericRepo<Address> _addressRepo = addressRepo;
        private readonly IGenericRepo<User> _userRepo = userRepo;
        private readonly IGenericRepo<Customer> _customerRepo = customerRepo;
        private readonly IGenericRepo<CustomerHistory> _auditRepo = auditRepo;
        private readonly IMapper _mapper = mapper;
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
            var oldCustomerDto = _mapper.Map<CustomerDto>(customer);
            var oldCustomer = JsonSerializer.Serialize(oldCustomerDto);
            var firstHistoryEntry = await _auditRepo.FirstOrDefaultAsync(h => h.CustomerId == customer.Id, ct);

            using var transaction = await _customerRepo.BeginTransactionAsync(ct);

            try
            {
                _addressRepo.Remove(address);
                await _addressRepo.SaveChangesAsync(ct);
                var newCustomerDto = _mapper.Map<CustomerDto>(customer);
                var auditEntry = CustomerHistory.UpdateCustomerHistory(customer.Id,
                                                                       user.UserName!,
                                                                       $"Delete Address with Id :{request.AddressId}",
                                                                       firstHistoryEntry.CreatedAt,
                                                                       firstHistoryEntry.CreatedBy,
                                                                       oldCustomer,
                                                                       JsonSerializer.Serialize(newCustomerDto));
                if (auditEntry.IsError)
                {
                    await transaction.RollbackAsync(ct);
                    _logger.LogError("Failed to create audit entry for deleting address with id {AddressId}", request.AddressId);
                    return Error.Failure("AuditEntryCreationFailed", $"Failed to create audit entry for deleting address with id {request.AddressId}");
                }
                await _auditRepo.AddAsync(auditEntry.Value, ct);
                await _auditRepo.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                _logger.LogError("Error occurred while deleting address with id {AddressId}", request.AddressId);
                return Error.Failure("DeleteAddressFailed", $"Error occurred while deleting address with id {request.AddressId}");
            }

            return Result.Deleted;
        }
    }
}
