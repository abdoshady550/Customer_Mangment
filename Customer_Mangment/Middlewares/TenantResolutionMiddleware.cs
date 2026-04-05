using Customer_Mangment.MultiTenancy;

namespace Customer_Mangment.Middlewares;

public sealed class TenantResolutionMiddleware(
    RequestDelegate next,
    TenantConnectionResolver resolver,
    ILogger<TenantResolutionMiddleware> logger)
{
    private const string HeaderName = "X-Tenant-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        if (ShouldBypass(context))
        {
            await next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(HeaderName, out var headerValues)
            || string.IsNullOrWhiteSpace(headerValues.FirstOrDefault()))
        {
            logger.LogWarning("Request to {Path} is missing the {Header} header.",
                context.Request.Path, HeaderName);

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                title = "Tenant not specified",
                detail = $"The '{HeaderName}' header is required."
            });
            return;
        }

        var tenantId = headerValues.First()!.Trim();
        var connectionString = resolver.Resolve(tenantId);

        if (connectionString is null)
        {
            logger.LogWarning("Unknown tenant '{TenantId}' requested {Path}.",
                tenantId, context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsJsonAsync(new
            {
                title = "Unknown tenant",
                detail = $"Tenant '{tenantId}' is not registered."
            });
            return;
        }

        context.Items["TenantId"] = tenantId;
        context.Items["TenantConnectionString"] = connectionString;

        var tenantContext = context.RequestServices.GetRequiredService<TenantContext>();
        tenantContext.Resolve(tenantId, connectionString);

        logger.LogDebug("Tenant resolved: {TenantId}", tenantId);

        await next(context);
    }

    private static bool ShouldBypass(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        return path.StartsWith("/health", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/alive", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/api/auth", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/openapi", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/scalar", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/hubs", StringComparison.OrdinalIgnoreCase);
    }
}