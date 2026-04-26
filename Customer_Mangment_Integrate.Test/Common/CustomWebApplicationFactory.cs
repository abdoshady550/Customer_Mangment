using Customer_Mangment;
using Customer_Mangment.Repository.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using System.Security.Claims;
using System.Text.Json;

namespace Customer_Mangment_Integrate.Test.Common
{
    public class CustomWebApplicationFactory : WebApplicationFactory<IAssmblyMarker>
    {
        public readonly WebApplicationFactory<Customer_Mangment.IdentityServer.IMarkerIdentity>
            IdentityFactory;

        public CustomWebApplicationFactory()
        {
            IdentityFactory =
                new WebApplicationFactory<Customer_Mangment.IdentityServer.IMarkerIdentity>()
                    .WithWebHostBuilder(builder =>
                    {
                        builder.UseEnvironment("Development");
                        builder.ConfigureServices(services =>
                        {
                            services.Configure<OpenIddictServerAspNetCoreOptions>(options =>
                                options.DisableTransportSecurityRequirement = true);

                            services.Configure<OpenIddict.Server.OpenIddictServerOptions>(options =>
                            {
                                options.TokenEndpointUris.Clear();
                                options.TokenEndpointUris.Add(
                                    new Uri("/connect/token", UriKind.Relative));
                                options.EndSessionEndpointUris.Clear();
                                options.EndSessionEndpointUris.Add(
                                    new Uri("/connect/logout", UriKind.Relative));
                            });
                        });
                    });
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            builder.ConfigureServices(services =>
            {
                // 1. Replace token service with in-process version
                services.RemoveAll<IIdentityServerTokenService>();
                services.AddSingleton<IIdentityServerTokenService>(
                    _ => new InProcessIdentityServerTokenService(
                        IdentityFactory.CreateClient()));


                var validationHandlerDescriptor = services.FirstOrDefault(d =>
                    d.ImplementationType?.FullName?.Contains(
                        "OpenIddictValidationAspNetCoreHandler") == true);
                if (validationHandlerDescriptor != null)
                    services.Remove(validationHandlerDescriptor);

                var schemeDescriptors = services
                    .Where(d =>
                        d.ServiceType == typeof(IAuthenticationSchemeProvider) == false &&
                        (d.ImplementationType?.Namespace?.StartsWith("OpenIddict.Validation") == true ||
                         d.ServiceType?.Namespace?.StartsWith("OpenIddict.Validation") == true))
                    .ToList();
                foreach (var d in schemeDescriptors)
                    services.Remove(d);


                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme =
                        OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme =
                        OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
                })
                .AddScheme<TestAuthOptions, TestAuthHandler>(
                    OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme,
                    opts => opts.IdentityClientFactory = () => IdentityFactory.CreateClient());
            });
        }

        public HttpClient CreateIdentityClient() => IdentityFactory.CreateClient();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                IdentityFactory.Dispose();
            base.Dispose(disposing);
        }
    }

    //   Auth Options 

    public class TestAuthOptions : AuthenticationSchemeOptions
    {
        public Func<HttpClient>? IdentityClientFactory { get; set; }
    }

    public class TestAuthHandler : AuthenticationHandler<TestAuthOptions>
    {
        public TestAuthHandler(
            Microsoft.Extensions.Options.IOptionsMonitor<TestAuthOptions> options,
            Microsoft.Extensions.Logging.ILoggerFactory logger,
            System.Text.Encodings.Web.UrlEncoder encoder)
            : base(options, logger, encoder) { }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return AuthenticateResult.NoResult();

            var token = authHeader["Bearer ".Length..].Trim();
            if (string.IsNullOrEmpty(token))
                return AuthenticateResult.NoResult();

            var factory = Options.IdentityClientFactory
                ?? throw new InvalidOperationException("IdentityClientFactory not configured");

            var client = factory();

            var introspectContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["token"] = token,
                ["client_id"] = "customer_api_resource",
                ["client_secret"] = "api-secret"
            });

            HttpResponseMessage introspectResponse;
            try
            {
                introspectResponse = await client.PostAsync(
                    "connect/introspect", introspectContent);
            }
            catch (Exception ex)
            {
                return AuthenticateResult.Fail(
                    $"Introspection call failed: {ex.Message}");
            }

            if (!introspectResponse.IsSuccessStatusCode)
                return AuthenticateResult.Fail(
                    $"Introspection returned {introspectResponse.StatusCode}");

            var body = await introspectResponse.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            if (!root.TryGetProperty("active", out var activeProp)
                || !activeProp.GetBoolean())
                return AuthenticateResult.Fail("Token is inactive");

            var claims = new List<Claim>();

            if (root.TryGetProperty("sub", out var sub) &&
                sub.ValueKind == JsonValueKind.String)
                claims.Add(new Claim(ClaimTypes.NameIdentifier, sub.GetString()!));

            if (root.TryGetProperty("email", out var email) &&
                email.ValueKind == JsonValueKind.String)
                claims.Add(new Claim(ClaimTypes.Email, email.GetString()!));

            if (root.TryGetProperty("name", out var name) &&
                name.ValueKind == JsonValueKind.String)
                claims.Add(new Claim(ClaimTypes.Name, name.GetString()!));

            if (root.TryGetProperty("role", out var roleProp))
            {
                if (roleProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var r in roleProp.EnumerateArray())
                        AddRoleClaims(claims, r.GetString());
                }
                else if (roleProp.ValueKind == JsonValueKind.String)
                {
                    AddRoleClaims(claims, roleProp.GetString());
                }
            }

            if (root.TryGetProperty("tenant_id", out var tid) &&
                tid.ValueKind == JsonValueKind.String)
                claims.Add(new Claim("tenant_id", tid.GetString()!));

            var identity = new ClaimsIdentity(
                claims,
                OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme,
                ClaimTypes.NameIdentifier,
                ClaimTypes.Role);

            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(
                principal,
                OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);

            return AuthenticateResult.Success(ticket);
        }

        private static void AddRoleClaims(List<Claim> claims, string? raw)
        {
            if (string.IsNullOrEmpty(raw)) return;

            // Always add the raw value
            claims.Add(new Claim(ClaimTypes.Role, raw));

            // If numeric, also add the name (mirrors RoleClaimTransformer)
            if (int.TryParse(raw, out var idx))
            {
                var named = idx switch { 0 => "User", 1 => "Admin", _ => null };
                if (named != null)
                    claims.Add(new Claim(ClaimTypes.Role, named));
            }
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = 401;
            return Task.CompletedTask;
        }

        protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = 403;
            return Task.CompletedTask;
        }
    }

    // ── In-process token service ──────────────────────────────────────────────

    internal sealed class InProcessIdentityServerTokenService : IIdentityServerTokenService
    {
        private readonly HttpClient _identityClient;

        public InProcessIdentityServerTokenService(HttpClient identityClient)
            => _identityClient = identityClient;

        public async Task<Customer_Mangment.CQRS.Identity.Dto.TokenServerResponse?>
            RequestPasswordTokenAsync(
                string email, string password, string? tenantId, CancellationToken ct)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "connect/token")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "password",
                    ["username"] = email,
                    ["password"] = password,
                    ["scope"] = "customer_api offline_access roles",
                    ["client_id"] = "customer-management-swagger"
                })
            };

            if (!string.IsNullOrWhiteSpace(tenantId))
                request.Headers.Add("X-Tenant-Id", tenantId);

            var response = await _identityClient.SendAsync(request, ct);
            var body = await response.Content.ReadAsStringAsync(ct);
            if (!response.IsSuccessStatusCode) return null;

            return JsonSerializer.Deserialize<
               Customer_Mangment.CQRS.Identity.Dto.TokenServerResponse>(
               body,
               new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<Customer_Mangment.CQRS.Identity.Dto.TokenServerResponse?>
            RefreshTokenAsync(string refreshToken, string? tenantId, CancellationToken ct)
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken,
                ["client_id"] = "customer-management-swagger"
            });

            var response = await _identityClient.PostAsync("connect/token", content, ct);
            var body = await response.Content.ReadAsStringAsync(ct);
            if (!response.IsSuccessStatusCode) return null;

            return JsonSerializer.Deserialize<
                Customer_Mangment.CQRS.Identity.Dto.TokenServerResponse>(
                body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}