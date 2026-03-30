using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace Customer_Mangment.OpenApi
{
    internal sealed class VersionInfoTransformer : IOpenApiDocumentTransformer
    {
        public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
        {
            var version = context.DocumentName;

            document.Info.Version = version;
            document.Info.Title = $"CustomerMangement API {version}";

            foreach (var path in document.Paths.Values)
            {
                foreach (var operation in path.Operations.Values)
                {
                    if (!string.IsNullOrEmpty(operation.OperationId))
                    {
                        operation.OperationId = $"{version}_{operation.OperationId}";
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
