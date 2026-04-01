namespace Customer_Mangment.Extensions
{
    public static class LocalizationExtensions
    {
        public static IServiceCollection AddAppLocalization(this IServiceCollection services)
        {
            services.AddLocalization(options =>
                options.ResourcesPath = "");

            return services;
        }
    }
}
