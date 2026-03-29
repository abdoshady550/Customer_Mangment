using Asp.Versioning;

namespace Customer_Mangment.Extensions
{
    public static class ApiVersioningExtension
    {
        public static IServiceCollection AddApiVersion(this IServiceCollection services)
        {
            services.AddApiVersioning(option =>
            {
                option.DefaultApiVersion = new ApiVersion(1, 0);
                option.AssumeDefaultVersionWhenUnspecified = true;
                option.ReportApiVersions = true;
                option.ApiVersionReader = new HeaderApiVersionReader("api-version");
            });
            return services;
        }
    }
}
