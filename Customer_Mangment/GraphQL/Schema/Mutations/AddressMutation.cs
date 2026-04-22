using Customer_Mangment.CQRS.Customers.Addresses.Commands.CreateAddress;
using Customer_Mangment.CQRS.Customers.Addresses.DTOS;
using Customer_Mangment.GraphQL.Schema.Inputs;
using Customer_Mangment.Repository.Interfaces.AppMediator;
using HotChocolate.Authorization;
using System.Security.Claims;

namespace Customer_Mangment.GraphQL.Schema.Mutations
{
    [ExtendObjectType("Mutation")]

    public sealed class AddressMutation
    {
        [Authorize]
        [GraphQLDescription("Add an address to an existing customer. Requires Admin role.")]
        [Authorize(Roles = ["Admin"])]
        public async Task<AddressDto> AddAddress(
            AddAddressInput input,
            [Service] IDispatcher dispatcher,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken ct)
        {
            var userId = RequireUserId(claimsPrincipal);

            var result = await dispatcher.Send(
                new AddAddressCommand(userId, input.CustomerId, input.Type, input.Value), ct);

            return result.IsError ? ThrowGql<AddressDto>(result.Errors) : result.Value;
        }

        private static string RequireUserId(ClaimsPrincipal p)
            => p.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? throw new GraphQLException(
                   ErrorBuilder.New().SetMessage("Not authenticated.").SetCode("UNAUTHORIZED").Build());

        private static T ThrowGql<T>(List<Model.Results.Error> errors)
            => throw new GraphQLException(
                errors.Select(e => ErrorBuilder.New()
                    .SetMessage(e.Description)
                    .SetCode(e.Code)
                    .Build()).ToList());
    }
}
