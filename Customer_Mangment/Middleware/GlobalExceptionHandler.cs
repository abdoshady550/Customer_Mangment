using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json;

namespace Customer_Mangment.Middleware
{
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger = logger;

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "An unhandled exception occurred");

            var problemDetails = CreateProblemDetails(exception);

            httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
            httpContext.Response.ContentType = "application/json";

            var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            await httpContext.Response.WriteAsync(json, cancellationToken);
            return true;
        }

        private ValidationProblemDetails CreateProblemDetails(Exception exception)
        {
            var modelStateDictionary = new ModelStateDictionary();
            var statusCode = StatusCodes.Status500InternalServerError;

            // Handle Identity errors specially
            if (exception.Message.Contains("PasswordRequires") ||
                exception.Message.Contains("Password") ||
                exception.Data.Contains("IdentityErrors"))
            {
                statusCode = StatusCodes.Status400BadRequest;
                ParseIdentityErrors(exception.Message, modelStateDictionary);
            }
            else
            {
                // Generic error handling
                var errorCode = exception.GetType().Name.Replace("Exception", "");
                modelStateDictionary.AddModelError(errorCode, exception.Message);
            }

            return new ValidationProblemDetails(modelStateDictionary)
            {
                Type = GetRfcUri(statusCode),
                Title = GetDefaultTitle(statusCode),
                Status = statusCode,
            };
        }

        private void ParseIdentityErrors(string message, ModelStateDictionary modelState)
        {


            var errors = message.Split(" | ");
            foreach (var error in errors)
            {
                var parts = error.Split(": ");
                if (parts.Length >= 2)
                {
                    var code = parts[0].Trim();
                    var description = parts[1].Trim();
                    modelState.AddModelError(code, description);
                }
            }
        }

        private static string GetRfcUri(int statusCode)
        {
            return statusCode switch
            {
                400 => "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                401 => "https://tools.ietf.org/html/rfc9110#section-15.5.2",
                404 => "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                409 => "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                500 => "https://tools.ietf.org/html/rfc9110#section-15.6.1",
                _ => "https://tools.ietf.org/html/rfc9110#section-15.5.1"
            };
        }

        private static string GetDefaultTitle(int statusCode)
        {
            return statusCode switch
            {
                400 => "One or more validation errors occurred.",
                401 => "Unauthorized access.",
                404 => "Resource not found.",
                409 => "Conflict occurred.",
                500 => "An internal server error occurred.",
                _ => "One or more validation errors occurred."
            };
        }
    }
}
