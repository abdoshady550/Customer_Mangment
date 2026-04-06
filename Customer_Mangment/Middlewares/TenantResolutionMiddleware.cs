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

        var requestedTenantId = headerValues.First()!.Trim();

        var connectionString = resolver.Resolve(requestedTenantId);
        if (connectionString is null)
        {
            logger.LogWarning("Unknown tenant '{TenantId}' requested {Path}.",
                requestedTenantId, context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsJsonAsync(new
            {
                title = "Unknown tenant",
                detail = $"Tenant '{requestedTenantId}' is not registered."
            });
            return;
        }

        context.Items["TenantId"] = requestedTenantId;
        context.Items["TenantConnectionString"] = connectionString;

        var tenantContext = context.RequestServices.GetRequiredService<TenantContext>();
        tenantContext.Resolve(requestedTenantId, connectionString);

        logger.LogDebug("Tenant resolved: {TenantId}", requestedTenantId);

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

public sealed class TenantClaimValidationMiddleware(RequestDelegate next,
    ILogger<TenantClaimValidationMiddleware> logger)
{
    private static readonly HashSet<string> _bypassPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/health", "/alive", "/api/auth", "/openapi", "/swagger", "/scalar", "/hubs"
    };

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        var shouldBypass = _bypassPaths.Any(p =>
            path.StartsWith(p, StringComparison.OrdinalIgnoreCase));

        if (shouldBypass)
        {
            await next(context);
            return;
        }

        if (context.User.Identity?.IsAuthenticated == true)
        {
            var claimTenantId = context.User.FindFirst("tenant_id")?.Value;
            var requestedTenantId = context.Items["TenantId"] as string;

            if (string.IsNullOrWhiteSpace(claimTenantId))
            {
                logger.LogWarning(
                    "JWT missing tenant_id claim for path {Path}", context.Request.Path);

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new
                {
                    title = "Tenant claim missing",
                    detail = "Your token does not contain a tenant claim. Please re-authenticate."
                });
                return;
            }

            if (!string.Equals(claimTenantId, requestedTenantId,
                    StringComparison.OrdinalIgnoreCase))
            {
                logger.LogWarning(
                    "Tenant mismatch. Claim={Claim} Header={Header} Path={Path}",
                    claimTenantId, requestedTenantId, context.Request.Path);

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new
                {
                    title = "Tenant access denied",
                    detail = "You are not authorized to access this tenant."
                });
                return;
            }
        }

        await next(context);
    }
}
