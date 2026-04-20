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

        services.AddHttpClient("OpenIddict.Validation.SystemNetHttp")
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });

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

                options.UseSystemNetHttp()
                       .ConfigureHttpClientHandler(_ => new HttpClientHandler
                       {
                           ServerCertificateCustomValidationCallback =
                               HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                       });
                options.Configure(o => o.TokenValidationParameters.RoleClaimType = "role");

                options.UseAspNetCore();
            });

        return services;
    }
}