using Customer_Mangment.Middlewares;
using Customer_Mangment.MultiTenancy;

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
        services.AddScoped<TenantDatabaseProvisioner>();

        services.AddScoped<TenantDbContextFactory>();

        return services;
    }


    public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder app)
        => app.UseMiddleware<TenantResolutionMiddleware>();
}