using Asp.Versioning.Conventions;
using Customer_Mangment.CQRS.Customers.DTOS;
using Customer_Mangment.Model.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Customer_Mangment.Endpoints.Customer
{
    public static class CustomerGroup
    {

        public static RouteGroupBuilder MapCustomerGroupEndpoints(this IEndpointRouteBuilder app)

        {
            var versionSet = app.NewApiVersionSet()
                .HasApiVersion(1)
                .HasApiVersion(2)
                .ReportApiVersions()
                .Build();

            var group = app.MapGroup("/api/customer-groups")
                .WithTags("Customer Groups")
                .WithApiVersionSet(versionSet)
                .WithOpenApi();

            group.MapGet("/customer-by-id", CustomerExtensions.GetCustomerById)
                .RequireAuthorization(policy => policy.RequireRole(nameof(Role.Admin)))
                .MapToApiVersion(1)

                .Produces<List<CustomerDto>>(StatusCodes.Status200OK)
                .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
                .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
                .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
                .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
                .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)

                .WithName("GetCustomerById")
                .WithSummary("Get Customer by id")
                .WithDescription(" retrieves the specific customer with that ID.");

            return group;
        }

    }
}
