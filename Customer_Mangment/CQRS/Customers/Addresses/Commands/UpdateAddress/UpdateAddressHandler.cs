using AutoMapper;
using Customer_Mangment.CQRS.Customers.DTOS;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces;
using MediatR;
using System.Text.Json;

namespace Customer_Mangment.CQRS.Customers.Addresses.Commands.UpdateAddress
{
    public sealed class UpdateAddressHandler(IGenericRepo<User> userRepo,
                                             IGenericRepo<Customer> customerRepo,
                                             IGenericRepo<Address> adressRepo,
                                             IGenericRepo<CustomerHistory> auditRepo,
                                             IMapper mapper,
                                             ILogger<UpdateAddressHandler> logger) : IRequestHandler<UpdateAddressCommand, Result<Updated>>

    {
        private readonly IGenericRepo<User> _userRepo = userRepo;
        private readonly IGenericRepo<Customer> _customerRepo = customerRepo;
        private readonly IGenericRepo<Address> _adressRepo = adressRepo;
        private readonly IGenericRepo<CustomerHistory> _auditRepo = auditRepo;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<UpdateAddressHandler> _logger = logger;

        public async Task<Result<Updated>> Handle(UpdateAddressCommand request, CancellationToken ct)
        {
            var user = await _userRepo.FirstOrDefaultAsync(u => u.Id == request.UserId, ct);
            if (user == null)
            {
                _logger.LogWarning("User with id {UserId} not found", request.UserId);
                return Error.NotFound("UserNotFound", $"User with id {request.UserId} not found");
            }
            var address = await _adressRepo.FirstOrDefaultAsync(a => a.Id == request.AddressId, ct);
            if (address == null)
            {
                _logger.LogWarning("Address with id {AddressId} not found", request.AddressId);
                return Error.NotFound("AddressNotFound", $"Address with id {request.AddressId} not found");
            }
            var repeatedAddress = await _adressRepo.AnyAsync(a => a.CustomerId == address.CustomerId && a.Type == request.Type && a.Id != address.Id, ct);
            if (repeatedAddress)
            {
                _logger.LogWarning("Address of type {AddressType} already exists for customer with id {CustomerId}", request.Type, address.CustomerId);
                return Error.Conflict("DuplicateAddress", $"Address of type {request.Type} already exists for customer with id {address.CustomerId}");
            }
            var customer = await _customerRepo.Include(c => c.Addresses).FirstOrDefaultAsync(c => c.Id == address.CustomerId, ct);
            if (customer == null)
            {
                _logger.LogWarning("Customer with id {CustomerId} not found", address.CustomerId);
                return Error.NotFound("CustomerNotFound", $"Customer with id {address.CustomerId} not found");
            }
            var oldCustomerDto = _mapper.Map<CustomerDto>(customer);
            var oldCustomerData = JsonSerializer.Serialize(oldCustomerDto);
            var updateResult = address.UpdateAddress(request.Type, request.Value);
            if (updateResult.IsError)
            {
                _logger.LogWarning("Failed to update address with id {AddressId}: {Errors}", request.AddressId, string.Join(", ", updateResult.Errors.Select(e => e.Description)));
                return updateResult.Errors;
            }
            var firstHistoryEntry = await _auditRepo.FirstOrDefaultAsync(h => h.CustomerId == customer.Id, ct);

            using var transaction = await _adressRepo.BeginTransactionAsync(ct);

            try
            {
                _adressRepo.Update(address);
                await _adressRepo.SaveChangesAsync(ct);

                var newCustomerDto = _mapper.Map<CustomerDto>(customer);

                var newCustomerData = JsonSerializer.Serialize(newCustomerDto);

                var auditEntry = CustomerHistory.UpdateCustomerHistory(customer.Id,
                                                                       user.UserName,
                                                                       $"Updated address with id {address.Id}",
                                                                       firstHistoryEntry.CreatedAt,
                                                                       firstHistoryEntry.CreatedBy,
                                                                       oldCustomerData,
                                                                       newCustomerData);
                if (auditEntry.IsError)
                {
                    await transaction.RollbackAsync(ct);
                    _logger.LogWarning("Failed to create audit entry for updating address with id {AddressId}: {Errors}", request.AddressId, string.Join(", ", auditEntry.Errors.Select(e => e.Description)));
                    return auditEntry.Errors;
                }
                await _auditRepo.AddAsync(auditEntry.Value, ct);
                await _auditRepo.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                _logger.LogError("An error occurred while updating address with id {AddressId}", request.AddressId);
                return Error.Failure("UpdateFailed", $"An error occurred while updating address with id {request.AddressId}");
            }

            return Result.Updated;
        }
    };
}
