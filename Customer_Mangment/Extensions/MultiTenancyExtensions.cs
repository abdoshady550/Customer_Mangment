using Customer_Mangment.Middlewares;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.MultiTenancy;
using Customer_Mangment.Repository.Interfaces;

namespace Customer_Mangment.Extensions;


public static class MultiTenancyExtensions
{
    public static IServiceCollection AddMultiTenancy(
        this IServiceCollection services,
        IConfiguration configuration)
    {

        services.AddSingleton<TenantConnectionResolver>();

        services.AddScoped<TenantContext>();
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());

        services.AddScoped<TenantDbContextFactory>();


        services.AddScoped<IGenericRepo<Customer>, TenantGenericRepo<Customer>>();
        services.AddScoped<IGenericRepo<Address>, TenantGenericRepo<Address>>();

        return services;
    }


    public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder app)
        => app.UseMiddleware<TenantResolutionMiddleware>();
}