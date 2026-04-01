using Asp.Versioning;
using Customer_Mangment.SharedResources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Net.Http.Headers;
using System.Text;

namespace Customer_Mangment.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]


    public class RabbitMQ_ManagementController(HttpClient client, IStringLocalizer<SharedResource> localizer) : ApiController(localizer)
    {
        private readonly HttpClient _client = client;

        [HttpGet]
        [Route("health")]
        [EndpointSummary("RabbitMQ Health Check")]
        [EndpointDescription("Checks the health status of the RabbitMQ server.")]
        public async Task<IActionResult> CheckRabbitMQHealth(CancellationToken ct)
        {
            try
            {
                var request = new HttpRequestMessage(
                              HttpMethod.Get,
                              "http://localhost:15672/api/overview");

                var byteArray = Encoding.ASCII.GetBytes("guest:guest");

                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                var response = await _client.SendAsync(request, ct);

                var content = await response.Content.ReadAsStringAsync(ct);

                if (response.IsSuccessStatusCode)
                {
                    return Content(content, "application/json");
                }

                return StatusCode((int)response.StatusCode, content);

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Details = ex.Message });
            }
        }

        [HttpGet]
        [Route("connections")]
        [EndpointSummary("RabbitMQ Connections")]
        [EndpointDescription("Get the Connections of the RabbitMQ server.")]
        public async Task<IActionResult> GetRabbitMQConnections(CancellationToken ct)
        {
            try
            {
                var request = new HttpRequestMessage(
                              HttpMethod.Get,
                              "http://localhost:15672/api/connections");

                var byteArray = Encoding.ASCII.GetBytes("guest:guest");

                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                var response = await _client.SendAsync(request, ct);

                var content = await response.Content.ReadAsStringAsync(ct);

                if (response.IsSuccessStatusCode)
                {
                    return Content(content, "application/json");
                }

                return StatusCode((int)response.StatusCode, content);

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Details = ex.Message });
            }
        }
        [HttpGet]
        [Route("queues")]
        [EndpointSummary("RabbitMQ Queues")]
        [EndpointDescription("Get the Queues of the RabbitMQ server.")]
        public async Task<IActionResult> GetRabbitMQQueues(CancellationToken ct)
        {
            try
            {
                var request = new HttpRequestMessage(
                              HttpMethod.Get,
                              "http://localhost:15672/api/queues");
                var byteArray = Encoding.ASCII.GetBytes("guest:guest");
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                var response = await _client.SendAsync(request, ct);
                var content = await response.Content.ReadAsStringAsync(ct);
                if (response.IsSuccessStatusCode)
                {
                    return Content(content, "application/json");
                }
                return StatusCode((int)response.StatusCode, content);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Details = ex.Message });
            }
        }
    }
}
