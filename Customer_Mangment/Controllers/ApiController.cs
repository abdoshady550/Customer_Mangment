using Customer_Mangment.Model.Results;
using Customer_Mangment.SharedResources;
using Customer_Mangment.SharedResources.Keys;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;

namespace Customer_Mangment.Controllers;

[ApiController]
public class ApiController(IStringLocalizer<SharedResource> localizer) : ControllerBase
{
    private readonly IStringLocalizer<SharedResource> _l = localizer;

    [NonAction]
    protected ActionResult Problem(List<Error> errors)
    {
        if (errors.Count is 0)
        {
            return Problem();
        }

        return ValidationProblem(errors);
    }

    private ActionResult ValidationProblem(List<Error> errors)
    {
        var modelStateDictionary = new ModelStateDictionary();
        var statusCode = StatusCodes.Status400BadRequest;

        if (errors.Any(e => e.Type == ErrorKind.NotFound))
        {
            statusCode = StatusCodes.Status404NotFound;
        }
        else if (errors.Any(e => e.Type == ErrorKind.Unauthorized))
        {
            statusCode = StatusCodes.Status401Unauthorized;
        }
        else if (errors.Any(e => e.Type == ErrorKind.Conflict))
        {
            statusCode = StatusCodes.Status409Conflict;
        }
        else if (errors.Any(e => e.Type == ErrorKind.Failure))
        {
            statusCode = StatusCodes.Status500InternalServerError;
        }

        foreach (var error in errors)
        {
            modelStateDictionary.AddModelError(error.Code, error.Description);
        }

        var problemDetails = new ValidationProblemDetails(modelStateDictionary)
        {
            Type = GetRfcUri(statusCode),
            Title = GetLocalizedTitle(statusCode),
            Status = statusCode,
        };

        return new ObjectResult(problemDetails)
        {
            StatusCode = statusCode
        };
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
    // localized titles 
    private string GetLocalizedTitle(int statusCode) => statusCode switch
    {
        400 => _l[ResourceKeys.General.ValidationErrors],
        401 => _l[ResourceKeys.Auth.Unauthorized],
        404 => _l[ResourceKeys.General.NotFound],
        409 => _l[ResourceKeys.General.Conflict],
        500 => _l[ResourceKeys.General.InternalServerError],
        _ => _l[ResourceKeys.General.ValidationErrors],
    };
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