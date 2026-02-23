using AutoMapper;
using Customer_Mangment.CQRS.Customers.DTOS;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Entities.History;
using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces;
using Customer_Mangment.Repository.Interfaces.AppMediator;

namespace Customer_Mangment.CQRS.Customers.Queries.GetCustomers
{
    public sealed class GetCustomerHistoryQueryHandler(IGenericRepo<User> userRepo,
                                          IGenericRepo<Customer> customerRepo,
                                          IGenericRepo<Address> AddressRepo,
                                          IGenericRepo<CustomerHistory> customerHistoryRepo,
                                          IGenericRepo<AddressHistory> addressHistoryRepo,
                                          IMapper mapper,
                                          ILogger<GetCustomerHistoryQueryHandler> logger) : IAppRequestHandler<GetCustomerHistoryQuery, Result<CustomerAddressHistoryDto>>
    {
        private readonly IGenericRepo<User> _userRepo = userRepo;
        private readonly IGenericRepo<Customer> _customerRepo = customerRepo;
        private readonly IGenericRepo<Address> _addressRepo = AddressRepo;
        private readonly IGenericRepo<CustomerHistory> _customerHistoryRepo = customerHistoryRepo;
        private readonly IGenericRepo<AddressHistory> _addressHistoryRepo = addressHistoryRepo;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<GetCustomerHistoryQueryHandler> _logger = logger;
        public async Task<Result<CustomerAddressHistoryDto>> Handle(GetCustomerHistoryQuery request, CancellationToken ct)
        {
            var user = await _userRepo.FirstOrDefaultAsync(u => u.Id == request.UserId, ct);
            if (user == null)
                return Error.NotFound("UserNotFound", $"User with id {request.UserId} not found.");

            var customerHistory = await _customerHistoryRepo
                .Where(ch => ch.Id == request.CustomerId)
                .ToListAsync(ct);

            if (!customerHistory.Any())
                return Error.NotFound("CustomerHistoryNotFound", $"No history found for customer {request.CustomerId}.");

            var addressHistory = await _addressHistoryRepo
                .Where(ah => ah.CustomerId == request.CustomerId)
                .ToListAsync(ct);

            var result = new CustomerAddressHistoryDto
            {
                CustomerHistoryDtos = _mapper.Map<List<CustomerHistoryDto>>(customerHistory),
                AddressHistoryDtos = _mapper.Map<List<AddressHistoryDto>>(addressHistory)
            };

            return result;
        }
    }
}
