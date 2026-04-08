using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace Customer_Mangment.OpenApi
{
    internal sealed class DefaultHeadersOperationTransformer : IOpenApiOperationTransformer
    {
        public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
        {
            operation.Parameters ??= new List<OpenApiParameter>();

            // Accept-Language
            if (!operation.Parameters.Any(p => p.Name == "Accept-Language" && p.In == ParameterLocation.Header))
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "Accept-Language",
                    In = ParameterLocation.Header,
                    Required = false,
                    Description = "Choose prefer lang: ar or en",
                    Schema = new OpenApiSchema
                    {
                        Type = "string",
                        Default = new Microsoft.OpenApi.Any.OpenApiString("en")
                    },
                    Example = new Microsoft.OpenApi.Any.OpenApiString("en")
                });
            }

            // X-Tenant-Id
            if (!operation.Parameters.Any(p => p.Name == "X-Tenant-Id" && p.In == ParameterLocation.Header))
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "X-Tenant-Id",
                    In = ParameterLocation.Header,
                    Required = true,
                    Description = "Tenant identifier. like: demo",
                    Schema = new OpenApiSchema
                    {
                        Type = "string"
                    },
                    Example = new Microsoft.OpenApi.Any.OpenApiString("demo")
                });
            }

            return Task.CompletedTask;
        }
    }
}
