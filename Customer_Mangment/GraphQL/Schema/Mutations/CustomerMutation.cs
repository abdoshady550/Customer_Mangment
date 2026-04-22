using Customer_Mangment.CQRS.Customers.Addresses.Commands.CreateAddress;
using Customer_Mangment.CQRS.Customers.Commands.CreateCustomer;
using Customer_Mangment.CQRS.Customers.DTOS;
using Customer_Mangment.GraphQL.Schema.Inputs;
using Customer_Mangment.Repository.Interfaces.AppMediator;
using HotChocolate.Authorization;
using System.Security.Claims;

namespace Customer_Mangment.GraphQL.Schema.Mutations
{
    [ExtendObjectType("Mutation")]

    public sealed class CustomerMutation
    {
        [Authorize]
        [GraphQLDescription("Create a new customer with optional addresses.")]
        public async Task<CustomerDto> CreateCustomer(
            CreateCustomerInput input,
            [Service] IDispatcher dispatcher,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken ct)
        {
            var userId = RequireUserId(claimsPrincipal);

            var addresses = input.Addresses?
                .ConvertAll(a => new CreateAddressCommand(a.Type, a.Value))
                ?? [];

            var result = await dispatcher.Send(
                new CreateCustomerCommand(userId, input.Name, input.Mobile, addresses), ct);

            return result.IsError ? ThrowGql<CustomerDto>(result.Errors) : result.Value;
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
