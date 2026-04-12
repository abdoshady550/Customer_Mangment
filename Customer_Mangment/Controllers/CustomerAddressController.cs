using Asp.Versioning;
using Customer_Mangment.CQRS.Customers.Addresses.Commands.CreateAddress;
using Customer_Mangment.CQRS.Customers.Addresses.Commands.DeleteAddress;
using Customer_Mangment.CQRS.Customers.Addresses.Commands.UpdateAddress;
using Customer_Mangment.CQRS.Customers.Addresses.DTOS;
using Customer_Mangment.CQRS.Customers.DTOS;
using Customer_Mangment.CQRS.Customers.Queries.GetCustomers;
using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces.AppMediator;
using Customer_Mangment.Req;
using Customer_Mangment.SharedResources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace Customer_Mangment.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiVersion("1.0")]

    public class CustomerAddressController(IDispatcher sender, IStringLocalizer<SharedResource> localizer) : ApiController(localizer)
    {
        private readonly IDispatcher _sender = sender;
        private string GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet]
        [Route("get")]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
        [ProducesResponseType(typeof(List<AddressDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointSummary("GetCustomerAddresses")]
        [EndpointDescription("Retrieves all addresses . If an AddressId is provided, retrieves the specific address with that ID , If an CustomerId is provided, retrieves the specific addresses for the given customer.")]
        public async Task<IActionResult> GetCustomerAddresses([FromQuery] Guid? CustomerId, [FromQuery] Guid? AddressId, CancellationToken ct)
        {
            Console.WriteLine("controller visited");
            var result = await _sender.Send(new GetAddressQuery(GetCurrentUserId(), CustomerId, AddressId), ct);
            return result.Match(
               response => Ok(response),
               Problem);
        }

        [HttpPost]
        [Route("add")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointSummary("AddAddress")]
        [EndpointDescription("This endpoint allows the authenticated user with role admin to create a new address. The request body should contain the address details, including the type and value.")]
        public async Task<IActionResult> AddAddress([FromQuery] Guid CustomerId, [FromBody] AddAddressReq req, CancellationToken ct)
        {
            var result = await _sender.Send(new AddAddressCommand(GetCurrentUserId(), CustomerId, req.Type, req.Value), ct);
            return result.Match(
               response => Ok(response),
               Problem);
        }
        [HttpGet]
        [Route("history")]
        [ProducesResponseType(typeof(List<AddressHistoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointSummary("GetAddressHistory")]
        [EndpointDescription("Retrieves the Address history of a specific customer based on the provided CustomerId.")]
        public async Task<IActionResult> GetCustomerHistory([FromQuery] Guid CustomerId, CancellationToken ct)
        {
            var result = await _sender.Send(new GetAddressHistoryQuery(GetCurrentUserId(), CustomerId), ct);
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
        [EndpointSummary("UpdateAddress")]
        [EndpointDescription("This endpoint allows the authenticated user with role admin to update an existing one. The request body should contain the address details, including the address ID (for updates), type, and value.")]
        public async Task<IActionResult> UpdateAddress([FromQuery] Guid AddressId, [FromBody] UpdateAddressReq req, CancellationToken ct)
        {
            var result = await _sender.Send(new UpdateAddressCommand(GetCurrentUserId(), AddressId, req.Type, req.Value), ct);
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
        [EndpointSummary("DeleteAddress")]
        [EndpointDescription("This endpoint allows the authenticated user  with role admin to delete an existing address. The request should include the address ID as a query parameter.")]
        public async Task<IActionResult> DeleteAddress([FromQuery] Guid AddressId, CancellationToken ct)
        {
            var result = await _sender.Send(new DeleteAddressCommand(GetCurrentUserId(), AddressId), ct);
            return result.Match(
               response => Ok(response),
               Problem);
        }

    }
}
