using AutoMapper;
using Customer_Mangment.CQRS.Customers.Addresses.DTOS;
using Customer_Mangment.CQRS.Customers.DTOS;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces;
using MediatR;
using System.Text.Json;

namespace Customer_Mangment.CQRS.Customers.Addresses.Commands.CreateAddress
{
    public class CreateAddressHandler(IGenericRepo<User> userRepo,
                                             IGenericRepo<Customer> customerRepo,
                                             IGenericRepo<Address> adressRepo,
                                             IGenericRepo<CustomerHistory> auditRepo,
                                             IMapper mapper,
                                             ILogger<CreateAddressHandler> logger) : IRequestHandler<AddAddressCommand, Result<AddressDto>>

    {
        private readonly IGenericRepo<User> _userRepo = userRepo;
        private readonly IGenericRepo<Customer> _customerRepo = customerRepo;
        private readonly IGenericRepo<Address> _adressRepo = adressRepo;
        private readonly IGenericRepo<CustomerHistory> _auditRepo = auditRepo;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<CreateAddressHandler> _logger = logger;

        public async Task<Result<AddressDto>> Handle(AddAddressCommand request, CancellationToken ct)
        {
            var user = await _userRepo.FirstOrDefaultAsync(u => u.Id == request.UserId, ct);

            if (user == null)
            {
                _logger.LogWarning("User with id {UserId} not found", request.UserId);
                return Error.NotFound("UserNotFound", $"User with id {request.UserId} not found");
            }
            var customer = await _customerRepo.Include(c => c.Addresses).FirstOrDefaultAsync(c => c.Id == request.CustomerId, ct);
            if (customer == null)
            {
                _logger.LogWarning("Customer with id {CustomerId} not found", request.CustomerId);
                return Error.NotFound("CustomerNotFound", $"Customer with id {request.CustomerId} not found");
            }
            var oldCustomerDto = _mapper.Map<CustomerDto>(customer);
            var oldCustomerData = JsonSerializer.Serialize(oldCustomerDto);
            var address = customer.AddAddress(request.Type, request.Value);
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
                return Error.Conflict("DuplicateAddress", $"Address of type {request.Type} already exists for customer with id {address.Value.CustomerId}");
            }
            using var transaction = await _adressRepo.BeginTransactionAsync(ct);
            try
            {

                await _adressRepo.AddAsync(address.Value, ct);

                _customerRepo.Update(customer);
                await _customerRepo.SaveChangesAsync(ct);
                var firstHistoryEntry = await _auditRepo.FirstOrDefaultAsync(h => h.CustomerId == customer.Id, ct);

                var auditEntry = CustomerHistory.UpdateCustomerHistory(customer.Id,
                                                                  user.Id,
                                                                  $"Added address with id {address.Value.Id}",
                                                                  firstHistoryEntry.CreatedAt,
                                                                  firstHistoryEntry.CreatedBy,
                                                                  oldCustomerData,
                                                                  JsonSerializer.Serialize(_mapper.Map<CustomerDto>(customer)));

                if (auditEntry.IsError)
                {
                    await transaction.RollbackAsync(ct);
                    _logger.LogWarning("Failed to create audit entry for updating address with id {AddressId}: {Errors}", address.Value.Id, string.Join(", ", auditEntry.Errors.Select(e => e.Description)));
                    return auditEntry.Errors;
                }
                await _auditRepo.AddAsync(auditEntry.Value, ct);
                await _auditRepo.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                _logger.LogError("An error occurred while updating address with id {AddressId}", address.Value.Id);
                return Error.Failure("UpdateFailed", $"An error occurred while updating address with id {address.Value.Id}");
            }
            var addressDto = _mapper.Map<AddressDto>(address.Value);

            return addressDto;

            throw new NotImplementedException();
        }
    }
}
