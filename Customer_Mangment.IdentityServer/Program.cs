using Customer_Mangment.IdentityServer.Data;
using Customer_Mangment.IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.UseOpenIddict();
});

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// OpenIddict configuration
builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
               .UseDbContext<ApplicationDbContext>();
    })
    .AddServer(options =>
    {
        options.SetTokenEndpointUris("/connect/token");
        options.SetLogoutEndpointUris("/connect/logout");
        options.SetIntrospectionEndpointUris("/connect/introspect");

        options.AllowPasswordFlow()
               .AllowRefreshTokenFlow()
               .AllowClientCredentialsFlow();

        options.RegisterScopes(OpenIddictConstants.Scopes.OpenId,
                               OpenIddictConstants.Scopes.Profile,
                               OpenIddictConstants.Scopes.Email,
                               OpenIddictConstants.Scopes.Roles,
                               OpenIddictConstants.Scopes.OfflineAccess,
                               "customer_api");

        options.AddDevelopmentEncryptionCertificate()
               .AddDevelopmentSigningCertificate();

        options.UseAspNetCore()
               .EnableTokenEndpointPassthrough()
               .EnableLogoutEndpointPassthrough()
               .EnableStatusCodePagesIntegration();
    })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });


var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedAsync(scope.ServiceProvider);
}

app.MapOpenApi();
app.MapScalarApiReference();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

