using Asp.Versioning;
using Customer_Mangment.CQRS.Webhooks.Commands;
using Customer_Mangment.CQRS.Webhooks.DTOs;
using Customer_Mangment.CQRS.Webhooks.Queries;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.Repository.Interfaces.AppMediator;
using Customer_Mangment.SharedResources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace Customer_Mangment.Controllers;

[Route("api/webhooks")]
[Authorize(Roles = nameof(Role.Admin))]
[ApiVersion("1.0")]
public sealed class WebhookController(IDispatcher dispatcher, IStringLocalizer<SharedResource> localizer) : ApiController(localizer)
{
    private string UserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpPost]
    [EndpointSummary("Register a webhook subscription")]
    [EndpointDescription("Creates a new webhook subscription. " +
        "The response includes the signing secret — store it securely, it will not be shown again.")]
    [ProducesResponseType(typeof(WebhookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateWebhookRequest req, CancellationToken ct)
    {
        var result = await dispatcher.Send(
            new CreateWebhookCommand(UserId(), req.Url, req.Events), ct);

        return result.Match(
           response => Ok(response),
           Problem);
    }

    [HttpGet]
    [EndpointSummary("List active webhook subscriptions")]
    [ProducesResponseType(typeof(List<WebhookDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await dispatcher.Send(new GetWebhooksQuery(UserId()), ct);
        return result.Match(
           response => Ok(response),
           Problem);
    }

    [HttpDelete("{id:guid}")]
    [EndpointSummary("Deactivate a webhook subscription")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await dispatcher.Send(
            new DeleteWebhookCommand(UserId(), id), ct);

        return result.Match(
           response => Ok(response),
           Problem);
    }
}

public sealed record CreateWebhookRequest(string Url, string[] Events);