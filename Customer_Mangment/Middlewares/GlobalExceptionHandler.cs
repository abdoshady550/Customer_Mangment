using Customer_Mangment.SharedResources;
using Customer_Mangment.SharedResources.Keys;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using System.Text.Json;

namespace Customer_Mangment.Middlewares
{
    public class GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IStringLocalizer<SharedResource> localizer
    ) : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger = logger;
        private readonly IStringLocalizer<SharedResource> _l = localizer;

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
            var modelState = new ModelStateDictionary();
            var statusCode = StatusCodes.Status500InternalServerError;

            // FluentValidation errors
            if (exception is FluentValidation.ValidationException validationException)
            {
                foreach (var error in validationException.Errors)
                {
                    modelState.AddModelError(
                        string.IsNullOrWhiteSpace(error.ErrorCode)
                            ? error.PropertyName
                            : error.ErrorCode,
                        error.ErrorMessage);
                }

                statusCode = StatusCodes.Status400BadRequest;
            }
            // Identity errors
            else if (exception.Message.Contains("PasswordRequires") ||
                     exception.Message.Contains("Password") ||
                     exception.Data.Contains("IdentityErrors"))
            {
                statusCode = StatusCodes.Status400BadRequest;
                ParseIdentityErrors(exception.Message, modelState);
            }

            // Generic errors
            else
            {
                var code = exception.GetType().Name.Replace("Exception", "");
                var message = LocalizedError.Failure(_l, code, code, exception.Message);
                modelState.AddModelError(code, message.Description);
            }

            return new ValidationProblemDetails(modelState)
            {
                Type = GetRfcUri(statusCode),
                Title = GetLocalizedTitle(statusCode),
                Status = statusCode
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
                    var localized = LocalizedError.Failure(_l, code, code, parts[1].Trim());
                    modelState.AddModelError(code, localized.Description);
                }
            }
        }

        private static string GetRfcUri(int statusCode) => statusCode switch
        {
            400 => "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            401 => "https://tools.ietf.org/html/rfc9110#section-15.5.2",
            404 => "https://tools.ietf.org/html/rfc9110#section-15.5.5",
            409 => "https://tools.ietf.org/html/rfc9110#section-15.5.10",
            500 => "https://tools.ietf.org/html/rfc9110#section-15.6.1",
            _ => "https://tools.ietf.org/html/rfc9110#section-15.5.1"
        };

        private string GetLocalizedTitle(int statusCode) => statusCode switch
        {
            400 => _l[ResourceKeys.General.ValidationErrors],
            401 => _l[ResourceKeys.Auth.Unauthorized],
            404 => _l[ResourceKeys.General.NotFound],
            409 => _l[ResourceKeys.General.Conflict],
            500 => _l[ResourceKeys.General.InternalServerError],
            _ => _l[ResourceKeys.General.ValidationErrors]
        };
    }
}