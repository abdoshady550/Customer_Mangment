using Asp.Versioning;
using Customer_Mangment.CQRS.Customers.Queries.Report;
using Customer_Mangment.Repository.Interfaces.AppMediator;
using Customer_Mangment.SharedResources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace Customer_Mangment.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    [ApiVersion("1.0")]

    public sealed class CustomerReportController(IDispatcher sender, IStringLocalizer<SharedResource> localizer) : ApiController(localizer)
    {
        private readonly IDispatcher _sender = sender;
        private string GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;


        [HttpGet("download")]
        [Authorize(Roles = "Admin")]
        [Produces("application/pdf")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointSummary("DownloadCustomerReport")]
        [EndpointDescription(
            "Generates and downloads a PDF report of all customers. " +
            "Pass optional 'from' / 'to' query parameters (UTC dates) to narrow the result set. " +
            "Requires the Admin role.")]
        [EndpointName("DownloadCustomerReport")]
        public async Task<IActionResult> DownloadReport(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            CancellationToken ct)
        {
            var result = await _sender.Send(
                new GetCustomerReportQuery(GetCurrentUserId(), from, to), ct);

            return result.Match(
                pdfBytes => File(pdfBytes, "application/pdf", BuildFileName(from, to)),
                Problem);
        }


        private static string BuildFileName(DateTime? from, DateTime? to)
        {
            var stamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");

            if (from.HasValue && to.HasValue)
                return $"customers_{from:yyyyMMdd}_{to:yyyyMMdd}_{stamp}.pdf";

            if (from.HasValue)
                return $"customers_from_{from:yyyyMMdd}_{stamp}.pdf";

            if (to.HasValue)
                return $"customers_until_{to:yyyyMMdd}_{stamp}.pdf";

            return $"customers_all_{stamp}.pdf";
        }
    }

}
