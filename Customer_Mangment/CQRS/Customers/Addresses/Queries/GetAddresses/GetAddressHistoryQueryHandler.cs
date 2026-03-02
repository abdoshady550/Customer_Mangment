using Customer_Mangment.CQRS.Customers.Addresses.DTOS;
using Customer_Mangment.CQRS.Customers.Queries.GetCustomers;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces;
using Customer_Mangment.Repository.Interfaces.AppMediator;
using Microsoft.EntityFrameworkCore;

namespace Customer_Mangment.CQRS.Customers.Addresses.Queries.GetAddresses
{
    public sealed class GetAddressHistoryQueryHandler(IGenericRepo<User> userRepo,
                                          IGenericRepo<Address> addressRepo,
                                          ILogger<GetCustomerHistoryQueryHandler> logger) : IAppRequestHandler<GetAddressHistoryQuery, Result<List<AddressDto>>>
    {
        private readonly IGenericRepo<User> _userRepo = userRepo;
        private readonly IGenericRepo<Address> _addressRepo = addressRepo;
        private readonly ILogger<GetCustomerHistoryQueryHandler> _logger = logger;
        public async Task<Result<List<AddressDto>>> Handle(GetAddressHistoryQuery request, CancellationToken ct)
        {
            var user = await _userRepo.FirstOrDefaultAsync(u => u.Id == request.UserId, ct);
            if (user == null)
                return Error.NotFound("UserNotFound", $"User with id {request.UserId} not found.");

            var addressHistory = await _addressRepo
                .TemporalAll()
                .Where(a => a.CustomerId == request.CustomerId)
                .Select(a => new AddressDto(
                    a.Id,
                    a.CustomerId,
                    a.Type,
                    a.Value
                ))
                .ToListAsync(ct);

            if (!addressHistory.Any())
                return Error.NotFound("CustomerHistoryNotFound", $"No history found for customer {request.CustomerId}.");



            return addressHistory;
        }
    }
}
