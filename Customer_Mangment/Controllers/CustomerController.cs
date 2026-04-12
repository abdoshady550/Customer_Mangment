using Asp.Versioning;
using Customer_Mangment.CQRS.Customers.Addresses.Commands.CreateAddress;
using Customer_Mangment.CQRS.Customers.Commands.CreateCustomer;
using Customer_Mangment.CQRS.Customers.Commands.DeleteCustomer;
using Customer_Mangment.CQRS.Customers.Commands.UpdateCustomer;
using Customer_Mangment.CQRS.Customers.DTOS;
using Customer_Mangment.CQRS.Customers.Queries.GetCustomers;
using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces.AppMediator;
using Customer_Mangment.Req;
using Customer_Mangment.SharedResources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace Customer_Mangment.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [EnableRateLimiting(policyName: "DefaultPolicy")]
    [ApiVersion("1.0")]

    public class CustomerController(IDispatcher sender, IStringLocalizer<SharedResource> localizer, IOutputCacheStore outputCacheStore)
        : ApiController(localizer)
    {
        private readonly IDispatcher _sender = sender;
        private string GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet]
        [Route("get")]
        [OutputCache(PolicyName = "customers")]
        [ProducesResponseType(typeof(List<CustomerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointSummary("GetCustomers")]
        [EndpointDescription("Retrieves a list of customers. If a CustomerId is provided, retrieves the specific customer with that ID.")]
        public async Task<IActionResult> GetCustomers([FromQuery] Guid? CustomerId, CancellationToken ct)
        {
            Console.WriteLine("controller visited");
            var result = await _sender.Send(new GetCustomersQuery(GetCurrentUserId(), CustomerId), ct);
            return result.Match(
               response => Ok(response),
               Problem);
        }
        [HttpGet]
        [Route("history")]
        [ProducesResponseType(typeof(List<CustomerHistoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointSummary("GetCustomerHistory")]
        [EndpointDescription("Retrieves the history of a specific customer based on the provided CustomerId.")]
        public async Task<IActionResult> GetCustomerHistory([FromQuery] Guid CustomerId, CancellationToken ct)
        {
            var result = await _sender.Send(new GetCustomerHistoryQuery(GetCurrentUserId(), CustomerId), ct);
            return result.Match(
               response => Ok(response),
               Problem);
        }
        [HttpPost]
        [Route("add")]
        [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointSummary("CreateCustomer")]
        [EndpointDescription("Creates a new customer with the provided details.")]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerReq req, CancellationToken ct)
        {
            var addresses = req.Adresses?.ConvertAll(a => new CreateAddressCommand(a.Type, a.Value));
            var result = await _sender.Send(new CreateCustomerCommand(GetCurrentUserId(), req.Name, req.Mobile, addresses), ct);
            return result.Match(
               response => Ok(response),
               Problem);
        }
        [HttpPut]
        [Route("update")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(Updated), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointSummary("UpdateCustomer")]
        [EndpointDescription("Updates an existing customer's details based on the provided information.")]
        public async Task<IActionResult> UpdateCustomer([FromQuery] Guid CustomerId, [FromBody] UpdateCustomerReq req, CancellationToken ct)
        {
            var result = await _sender.Send(new UpdateCustomerCommand(GetCurrentUserId(), CustomerId, req.Name, req.Mobile), ct);
            await outputCacheStore.EvictByTagAsync("CustomerCache", ct);

            return result.Match(
            response => Ok(response),
            Problem);
        }
        [HttpDelete]
        [Route("delete")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(Deleted), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointSummary("DeleteCustomer")]
        [EndpointDescription("Deletes a customer based on the provided CustomerId.")]
        public async Task<IActionResult> DeleteCustomer([FromQuery] Guid CustomerId, CancellationToken ct)
        {
            var result = await _sender.Send(new DeleteCustomerCommand(GetCurrentUserId(), CustomerId), ct);
            await outputCacheStore.EvictByTagAsync("CustomerCache", ct);
            return result.Match(
            response => Ok(response),
            Problem);
        }
    }
}
