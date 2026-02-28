using Customer_Mangment.CQRS.Customers.Addresses.DTOS;
using Customer_Mangment.CQRS.Customers.Mappers;
using Customer_Mangment.CQRS.Customers.Queries.GetCustomers;
using Customer_Mangment.Data;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces;
using Customer_Mangment.Repository.Interfaces.AppMediator;
using Microsoft.EntityFrameworkCore;

namespace Customer_Mangment.CQRS.Customers.Addresses.Queries.GetAddresses
{
    public sealed class GetAddressHistoryQueryHandler(IGenericRepo<User> userRepo,
                                          AppDbContext context,
                                          ICustomerMapper mapper,
                                          ILogger<GetCustomerHistoryQueryHandler> logger) : IAppRequestHandler<GetAddressHistoryQuery, Result<List<AddressDto>>>
    {
        private readonly IGenericRepo<User> _userRepo = userRepo;
        private readonly AppDbContext _context = context;
        private readonly ICustomerMapper _mapper = mapper;
        private readonly ILogger<GetCustomerHistoryQueryHandler> _logger = logger;
        public async Task<Result<List<AddressDto>>> Handle(GetAddressHistoryQuery request, CancellationToken ct)
        {
            var user = await _userRepo.FirstOrDefaultAsync(u => u.Id == request.UserId, ct);
            if (user == null)
                return Error.NotFound("UserNotFound", $"User with id {request.UserId} not found.");

            var addressHistory = await _context.Addresses
                .TemporalAll()
                .Where(a => a.CustomerId == request.CustomerId)
                .IgnoreQueryFilters()
                .AsNoTracking()
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
