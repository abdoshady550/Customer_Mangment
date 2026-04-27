using Customer_Mangment.CQRS.Customers.Addresses.DTOS;
using Customer_Mangment.CQRS.Customers.Mappers;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.Repository.Interfaces;
using Customer_Mangment.Repository.Interfaces.AppMediator;
using Customer_Mangment.SharedResources;
using Customer_Mangment.SharedResources.Keys;
using Microsoft.Extensions.Localization;

namespace Customer_Mangment.CQRS.Customers.Addresses.Queries.OData
{
    public sealed class ODataAddressesQueryHandler(
        IGenericRepo<User> userRepo,
        IGenericRepo<Address> addressRepo,
        ICustomerMapper mapper,
        IStringLocalizer<SharedResource> localizer,
        ILogger<ODataAddressesQueryHandler> logger)
        : IAppRequestHandler<ODataAddressesQuery, Model.Results.Result<IQueryable<AddressDto>>>
    {
        public async Task<Model.Results.Result<IQueryable<AddressDto>>> Handle(
            ODataAddressesQuery request, CancellationToken ct)
        {
            var user = await userRepo.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == request.UserId, ct);

            if (user is null)
                return LocalizedError.Unauthorized(localizer, "UserNotFound",
                    ResourceKeys.User.NotFound, request.UserId);

            var query = addressRepo.AsNoTracking();

            if (request.CustomerId.HasValue)
                query = query.Where(a => a.CustomerId == request.CustomerId.Value);

            var addresses = await query.ToListAsync(ct);
            var dtos = mapper.ToAddressDtoList(addresses);
            return Model.Results.Result<IQueryable<AddressDto>>.Ok(dtos.AsQueryable());
        }
    }
}
