using Customer_Mangment.Data;
using Microsoft.EntityFrameworkCore;

namespace Customer_Mangment.MultiTenancy;

public sealed class TenantDatabaseProvisioner(
    TenantConnectionResolver resolver,
    ILoggerFactory loggerFactory,
    ILogger<TenantDatabaseProvisioner> logger)
{
    public async Task EnsureAllTenantsProvisionedAsync(CancellationToken ct = default)
    {
        foreach (var tenantId in resolver.RegisteredTenants)
        {
            await EnsureTenantProvisionedAsync(tenantId, ct);
        }
    }

    public async Task EnsureTenantProvisionedAsync(string tenantId, CancellationToken ct = default)
    {
        var connectionString = resolver.Resolve(tenantId);
        if (connectionString is null)
        {
            logger.LogWarning("Skipping provisioning for unknown tenant '{TenantId}'.", tenantId);
            return;
        }

        logger.LogInformation("Provisioning database for tenant '{TenantId}'.", tenantId);

        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(connectionString)
                .UseLoggerFactory(loggerFactory)
                .Options;

            await using var context = new AppDbContext(options);

            await context.Database.EnsureCreatedAsync(ct);

            logger.LogInformation("Tenant '{TenantId}' database is ready.", tenantId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to provision database for tenant '{TenantId}'.", tenantId);
            throw;
        }
    }
}
