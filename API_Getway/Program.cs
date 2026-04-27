using OpenIddict.Validation.AspNetCore;
using Scalar.AspNetCore;


namespace API_Getway;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders("X-Tenant-Id");
            });
        });

        // ── OpenIddict 
        var authority = builder.Configuration["Auth:Authority"]
            ?? throw new InvalidOperationException("Auth:Authority is required.");
        var introspectionSecret = builder.Configuration["Auth:IntrospectionSecret"]
            ?? throw new InvalidOperationException("Auth:IntrospectionSecret is required.");

        builder.Services.AddHttpClient("OpenIddict.Validation.SystemNetHttp")
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme =
                OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme =
                OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
        });

        builder.Services.AddOpenIddict()
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

                options.Configure(o =>
                    o.TokenValidationParameters.RoleClaimType = "role");

                options.UseAspNetCore();
            });

        builder.Services.AddAuthorization();

        builder.Services.AddReverseProxy()
            .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

        var app = builder.Build();
        app.Use(async (context, next) =>
        {
            context.Request.EnableBuffering();

            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync();

            context.Request.Body.Position = 0;

            Console.WriteLine($"BODY: {body}");

            await next();
        });
        app.MapDefaultEndpoints();

        app.MapOpenApi();
        app.MapScalarApiReference();

        app.UseHttpsRedirection();

        app.UseCors();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapReverseProxy();

        app.MapControllers();

        app.Run();
    }
}