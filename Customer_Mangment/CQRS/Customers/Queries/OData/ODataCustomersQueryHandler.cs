using Customer_Mangment.CQRS.Customers.DTOS;
using Customer_Mangment.CQRS.Customers.Mappers;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.Repository.Interfaces;
using Customer_Mangment.Repository.Interfaces.AppMediator;
using Customer_Mangment.SharedResources;
using Customer_Mangment.SharedResources.Keys;
using Microsoft.Extensions.Localization;

namespace Customer_Mangment.CQRS.Customers.Queries.OData
{
    public sealed class ODataCustomersQueryHandler(
        IGenericRepo<User> userRepo,
        IGenericRepo<Customer> customerRepo,
        ICustomerMapper mapper,
        IStringLocalizer<SharedResource> localizer,
        ILogger<ODataCustomersQueryHandler> logger)
        : IAppRequestHandler<ODataCustomersQuery, Model.Results.Result<IQueryable<CustomerDto>>>
    {
        public async Task<Model.Results.Result<IQueryable<CustomerDto>>> Handle(
            ODataCustomersQuery request, CancellationToken ct)
        {
            var user = await userRepo.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == request.UserId, ct);

            if (user is null)
            {
                logger.LogWarning("User {UserId} not found", request.UserId);
                return LocalizedError.Unauthorized(localizer, "UserNotFound",
                    ResourceKeys.User.NotFound, request.UserId);
            }

            var customers = await customerRepo
                .AsNoTracking()
                .Include(c => c.Addresses)
                .ToListAsync(ct);

            var dtos = mapper.ToCustomerDtoList(customers);
            return Model.Results.Result<IQueryable<CustomerDto>>.Ok(dtos.AsQueryable());

        }
    }
}
