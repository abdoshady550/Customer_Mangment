using Customer_Mangment.Data;
using Microsoft.EntityFrameworkCore;

namespace Customer_Mangment.MultiTenancy;

public sealed class TenantDbContextFactory(
    ITenantContext tenantContext,
    ILoggerFactory loggerFactory)
{
    private AppDbContext? _context;
    public AppDbContext Create()
    {
        if (_context is not null)
            return _context;

        if (!tenantContext.IsResolved)
            throw new InvalidOperationException(
                "TenantDbContextFactory.Create() was called before the tenant was resolved. " +
                "Ensure TenantResolutionMiddleware runs before any tenant-scoped repository.");

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(tenantContext.ConnectionString)
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
