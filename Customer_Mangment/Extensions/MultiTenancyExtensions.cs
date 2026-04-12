using Customer_Mangment.Middlewares;
using Customer_Mangment.MultiTenancy;
using Customer_Mangment.Repository.Interfaces.tenantCache;
using Customer_Mangment.Repository.Services.tenantCache;

namespace Customer_Mangment.Extensions;


public static class MultiTenancyExtensions
{
    public static IServiceCollection AddMultiTenancy(
        this IServiceCollection services,
        IConfiguration configuration)
    {

        services.AddHttpContextAccessor();

        services.AddSingleton<TenantConnectionResolver>();

        services.AddScoped<TenantContext>();
        //services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());
        services.AddScoped<ITenantContext, TenantContextAccessor>();
        services.AddScoped<ITenantCachedQueryService, TenantCachedQueryService>();
        services.AddScoped<TenantDatabaseProvisioner>();

        services.AddScoped<TenantDbContextFactory>();

        return services;
    }


    public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder app)
        => app.UseMiddleware<TenantResolutionMiddleware>();
}