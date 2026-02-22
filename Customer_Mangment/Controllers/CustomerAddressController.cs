using Customer_Mangment.CQRS.Customers.Addresses.Commands.CreateAddress;
using Customer_Mangment.CQRS.Customers.Addresses.Commands.DeleteAddress;
using Customer_Mangment.CQRS.Customers.Addresses.Commands.UpdateAddress;
using Customer_Mangment.CQRS.Customers.Addresses.DTOS;
using Customer_Mangment.Model.Results;
using Customer_Mangment.Req;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Customer_Mangment.Controllers
{
    [Route("api/[controller]")]
    public class CustomerAddressController(ISender sender) : ApiController
    {
        private readonly ISender sender = sender;
        private string GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

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
            var result = await sender.Send(new AddAddressCommand(GetCurrentUserId(), CustomerId, req.Type, req.Value), ct);
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
        public async Task<IActionResult> UpdateAddress([FromBody] UpdateAddressReq req, CancellationToken ct)
        {
            var result = await sender.Send(new UpdateAddressCommand(GetCurrentUserId(), req.AddressId, req.Type, req.Value), ct);
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
            var result = await sender.Send(new DeleteAddressCommand(GetCurrentUserId(), AddressId), ct);
            return result.Match(
               response => Ok(response),
               Problem);
        }

    }
}
