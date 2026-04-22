using Customer_Mangment.CQRS.Customers.Addresses.DTOS;
using Customer_Mangment.CQRS.Customers.DTOS;
using Customer_Mangment.CQRS.Customers.Queries.GetCustomers;
using Customer_Mangment.Repository.Interfaces.AppMediator;
using Customer_Mangment.SharedResources;
using Customer_Mangment.SharedResources.Keys;
using HotChocolate.Authorization;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace Customer_Mangment.GraphQL.Schema.Queries
{
    [ExtendObjectType("Query")]
    public sealed class CustomerQuery
    {

        [Authorize]
        [GraphQLDescription("Retrieve all customers, or a specific one by ID.")]
        public async Task<List<CustomerDto>> GetCustomers(
            Guid? customerId,
            [Service] IStringLocalizer<SharedResource> l,
            [Service] IDispatcher dispatcher,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken ct)
        {
            var userId = RequireUserId(claimsPrincipal, l);
            var result = await dispatcher.Send(new GetCustomersQuery(userId, customerId), ct);

            return result.IsError ? ThrowGql<List<CustomerDto>>(result.Errors) : result.Value;
        }

        [Authorize]
        [GraphQLDescription("Retrieve addresses. Filter by CustomerId and/or AddressId.")]
        public async Task<List<AddressDto>> GetAddresses(
            Guid? customerId,
            Guid? addressId,
            [Service] IStringLocalizer<SharedResource> l,
            [Service] IDispatcher dispatcher,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken ct)
        {
            var userId = RequireUserId(claimsPrincipal, l);
            var result = await dispatcher.Send(new GetAddressQuery(userId, customerId, addressId), ct);
            return result.IsError ? ThrowGql<List<AddressDto>>(result.Errors) : result.Value;
        }


        private static string RequireUserId(ClaimsPrincipal p, IStringLocalizer L)
            => p.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? throw new GraphQLException(
                   ErrorBuilder.New().SetMessage(L[ResourceKeys.Auth.Unauthorized]).SetCode("UNAUTHORIZED").Build());

        private static T ThrowGql<T>(List<Model.Results.Error> errors)
            => throw new GraphQLException(
                errors.Select(e => ErrorBuilder.New()
                    .SetMessage(e.Description)
                    .SetCode(e.Code)
                    .Build()).ToList());
    }
}
