using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Validation.AspNetCore;

namespace Customer_Mangment.Extensions;

public static class AuthenticationExtension
{
    public static IServiceCollection AddOpenIddictTokenValidation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var authMode = configuration["Auth:Mode"] ?? "Jwt";
        var authority = configuration["Auth:Authority"] ?? "http://localhost:5100";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
        });

        var builder = services.AddOpenIddict()
            .AddValidation(options =>
            {
                options.SetIssuer(authority);
                options.UseSystemNetHttp();
                options.UseAspNetCore();

                if (authMode.Equals("Introspect", StringComparison.OrdinalIgnoreCase))
                {
                    options.UseIntrospection()
                           .SetClientId("customer_api_resource")
                           .SetClientSecret(configuration["Auth:IntrospectionSecret"]!);
                }

            });

        if (authMode == "Jwt")
        {
            services.AddAuthentication()
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.Authority = authority;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false,
                        ValidateIssuer = true,
                        ValidIssuer = authority,
                        ValidateLifetime = true,
                    };
                });
        }

        return services;
    }
}