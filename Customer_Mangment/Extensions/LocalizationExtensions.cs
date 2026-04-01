using Microsoft.AspNetCore.Localization;
using System.Globalization;

namespace Customer_Mangment.Extensions
{
    public static class LocalizationExtensions
    {
        public static IServiceCollection AddAppLocalization(this IServiceCollection services)
        {
            services.AddLocalization(options =>
                options.ResourcesPath = "");

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var defaultCulture = new CultureInfo("en");

                var supported = new[]
                {
                    new CultureInfo("en"),
                    new CultureInfo("en-US"),
                    new CultureInfo("ar"),
                    new CultureInfo("ar-EG"),
                };

                options.DefaultRequestCulture = new RequestCulture(defaultCulture);
                options.SupportedCultures = supported;
                options.SupportedUICultures = supported;


                options.FallBackToParentCultures = true;
                options.FallBackToParentUICultures = true;
            });

            return services;
        }
    }
}