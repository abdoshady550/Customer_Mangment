using AutoMapper;
using Customer_Mangment.CQRS.Customers.DTOS;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces;
using MediatR;

namespace Customer_Mangment.CQRS.Customers.Queries.GetCustomers
{
    public sealed class GetCustomerHistoryQueryHandler(IGenericRepo<User> userRepo,
                                          IGenericRepo<CustomerHistory> auditRepo,
                                          IMapper mapper,
                                          ILogger<GetCustomerHistoryQueryHandler> logger) : IRequestHandler<GetCustomerHistoryQuery, Result<List<CustomerHistoryDto>>>
    {
        private readonly IGenericRepo<User> _userRepo = userRepo;
        private readonly IGenericRepo<CustomerHistory> _auditRepo = auditRepo;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<GetCustomerHistoryQueryHandler> _logger = logger;

        public async Task<Result<List<CustomerHistoryDto>>> Handle(GetCustomerHistoryQuery request, CancellationToken ct)
        {
            var user = await _userRepo.FirstOrDefaultAsync(u => u.Id == request.UserId, ct);
            if (user == null)
            {
                _logger.LogWarning("User with id {UserId} not found.", request.UserId);
                return Error.NotFound("UserNotFound", $"User with id {request.UserId} not found.");
            }
            var history = await _auditRepo.AsNoTracking().Where(ch => ch.CustomerId == request.CustomerId).ToListAsync(ct);
            if (history == null)
            {
                _logger.LogWarning("Customer history for customer id {CustomerId} not found.", request.CustomerId);
                return Error.NotFound("CustomerHistoryNotFound", $"Customer history for customer id {request.CustomerId} not found.");
            }
            var historyDtos = _mapper.Map<List<CustomerHistoryDto>>(history);
            return historyDtos;
        }
    }
}
