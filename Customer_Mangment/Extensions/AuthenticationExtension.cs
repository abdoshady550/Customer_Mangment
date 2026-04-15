using OpenIddict.Validation.AspNetCore;

namespace Customer_Mangment.Extensions;

public static class AuthenticationExtension
{
    public static IServiceCollection AddOpenIddictTokenValidation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var authority = configuration["Auth:Authority"]
            ?? throw new InvalidOperationException("Auth:Authority is required.");

        var introspectionSecret = configuration["Auth:IntrospectionSecret"]
            ?? throw new InvalidOperationException("Auth:IntrospectionSecret is required.");

        // Set OpenIddict as the default scheme
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme =
                OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme =
                OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
        });

        services.AddOpenIddict()
            .AddValidation(options =>
            {
                options.SetIssuer(authority);

                options.UseIntrospection()
                       .SetClientId("customer_api_resource")
                       .SetClientSecret(introspectionSecret);

                options.UseSystemNetHttp();
                options.UseAspNetCore();

            });

        return services;
    }
}