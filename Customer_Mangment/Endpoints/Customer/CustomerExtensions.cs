using Customer_Mangment.CQRS.Customers.Queries.GetCustomers;
using Customer_Mangment.Repository.Interfaces.AppMediator;
using Microsoft.AspNetCore.Mvc;

namespace Customer_Mangment.Endpoints.Customer
{
    public static class CustomerExtensions
    {
        public static async Task<IResult> GetCustomerById([FromQuery] Guid id,
                                           IDispatcher sender,
                                           CancellationToken ct)
        {
            var result = await sender.Send(new GetCustomersQuery(GetCurrentUserId(), id), ct);

            return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound();
        }

        private static string GetCurrentUserId() => "11111111-1111-1111-1111-111111111111";

    }
}
