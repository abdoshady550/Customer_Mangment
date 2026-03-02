using Customer_Mangment.CQRS.Customers.DTOS;
using Customer_Mangment.CQRS.Customers.Mappers;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces;
using Customer_Mangment.Repository.Interfaces.AppMediator;

namespace Customer_Mangment.CQRS.Customers.Queries.GetCustomers
{
    public sealed class GetCustomerHistoryQueryHandler(IGenericRepo<User> userRepo,
                                          IGenericRepo<Customer> customerrepo,
                                          ICustomerMapper mapper,
                                          ILogger<GetCustomerHistoryQueryHandler> logger) : IAppRequestHandler<GetCustomerHistoryQuery, Result<List<CustomerDto>>>
    {
        private readonly IGenericRepo<User> _userRepo = userRepo;
        private readonly IGenericRepo<Customer> _customerrepo = customerrepo;
        private readonly ICustomerMapper _mapper = mapper;
        private readonly ILogger<GetCustomerHistoryQueryHandler> _logger = logger;
        public async Task<Result<List<CustomerDto>>> Handle(GetCustomerHistoryQuery request, CancellationToken ct)
        {
            var user = await _userRepo.FirstOrDefaultAsync(u => u.Id == request.UserId, ct);
            if (user == null)
                return Error.NotFound("UserNotFound", $"User with id {request.UserId} not found.");

            var customerHistory = await _customerrepo.TemporalAll().Where(c => c.Id == request.CustomerId).ToListAsync(ct);

            if (!customerHistory.Any())
                return Error.NotFound("CustomerHistoryNotFound", $"No history found for customer {request.CustomerId}.");

            var result = _mapper.ToCustomerDtoList(customerHistory);

            return result;
        }
    }
}
