using Customer_Mangment.Data;
using Microsoft.EntityFrameworkCore;

public sealed class TenantDbContextFactory(
    IHttpContextAccessor httpContextAccessor,
    ILoggerFactory loggerFactory)
{
    private AppDbContext? _context;

    public AppDbContext Create()
    {
        if (_context is not null)
            return _context;

        var httpContext = httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("No HTTP context available for tenant resolution.");

        var tenantId = httpContext.Items["TenantId"] as string;
        var connectionString = httpContext.Items["TenantConnectionString"] as string;

        if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "Tenant not resolved in current context. Ensure TenantResolutionMiddleware runs before any tenant-scoped repository.");
        }

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .UseLoggerFactory(loggerFactory)
            .Options;

        _context = new AppDbContext(options);
        return _context;
    }

    public async ValueTask DisposeAsync()
    {
        if (_context is not null)
            await _context.DisposeAsync();
    }
}