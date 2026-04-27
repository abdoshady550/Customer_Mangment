using Asp.Versioning;
using Customer_Mangment.CQRS.Customers.Addresses.DTOS;
using Customer_Mangment.CQRS.Customers.Addresses.Queries.OData;
using Customer_Mangment.CQRS.Customers.DTOS;
using Customer_Mangment.CQRS.Customers.Queries.OData;
using Customer_Mangment.Repository.Interfaces.AppMediator;
using Customer_Mangment.SharedResources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace Customer_Mangment.Controllers
{
    [Route("odata")]
    [Authorize]
    [ApiVersion("1.0")]
    public sealed class CustomerODataController(
        IDispatcher sender,
        IStringLocalizer<SharedResource> localizer) : ApiController(localizer)
    {
        private string GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpGet("Customers")]
        [EnableQuery(MaxTop = 100, AllowedQueryOptions =
            AllowedQueryOptions.Select |
            AllowedQueryOptions.Filter |
            AllowedQueryOptions.OrderBy |
            AllowedQueryOptions.Top |
            AllowedQueryOptions.Skip |
            AllowedQueryOptions.Count |
            AllowedQueryOptions.Expand)]
        [ProducesResponseType(typeof(IQueryable<CustomerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCustomers(CancellationToken ct)
        {
            var result = await sender.Send(
                new ODataCustomersQuery(GetCurrentUserId()), ct);

            return result.Match(
                queryable => Ok(queryable),
                Problem);
        }

        [HttpGet("Addresses")]
        [EnableQuery(MaxTop = 100, AllowedQueryOptions =
            AllowedQueryOptions.Select |
            AllowedQueryOptions.Filter |
            AllowedQueryOptions.OrderBy |
            AllowedQueryOptions.Top |
            AllowedQueryOptions.Skip |
            AllowedQueryOptions.Count)]
        [ProducesResponseType(typeof(IQueryable<AddressDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAddresses(
            [FromQuery] Guid? customerId, CancellationToken ct)
        {
            var result = await sender.Send(
                new ODataAddressesQuery(GetCurrentUserId(), customerId), ct);

            return result.Match(
                queryable => Ok(queryable),
                Problem);
        }
    }
}
