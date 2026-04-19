using Customer_Mangment;
using Customer_Mangment.CQRS.Identity.Dto;
using Customer_Mangment.Repository.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Text.Json;

namespace Customer_Mangment_Integrate.Test.Common
{

    public class CustomWebApplicationFactory
        : WebApplicationFactory<IAssmblyMarker>
    {
        private readonly WebApplicationFactory<Customer_Mangment.IdentityServer.IMarkerIdentity>
            _identityFactory;

        public CustomWebApplicationFactory()
        {
            _identityFactory =
                new WebApplicationFactory<Customer_Mangment.IdentityServer.IMarkerIdentity>()
                    .WithWebHostBuilder(builder =>
                    {
                        builder.UseEnvironment("Development");

                        builder.ConfigureServices(services =>
                        {
                            services.Configure<OpenIddict.Server.AspNetCore.OpenIddictServerAspNetCoreOptions>(options =>
                            {
                                options.DisableTransportSecurityRequirement = true;
                            });

                            services.Configure<OpenIddict.Server.OpenIddictServerOptions>(options =>
                            {
                                options.TokenEndpointUris.Clear();
                                options.TokenEndpointUris.Add(new Uri("/connect/token", UriKind.Relative));

                                options.EndSessionEndpointUris.Clear();
                                options.EndSessionEndpointUris.Add(new Uri("/connect/logout", UriKind.Relative));
                            });
                        });
                    });
        }


        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IIdentityServerTokenService>();
                services.AddSingleton<IIdentityServerTokenService>(
                    _ => new InProcessIdentityServerTokenService(
                        _identityFactory.CreateClient()));
            });
        }


        public HttpClient CreateIdentityClient() => _identityFactory.CreateClient();


        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _identityFactory.Dispose();

            base.Dispose(disposing);
        }
    }



    internal sealed class InProcessIdentityServerTokenService : IIdentityServerTokenService
    {
        private readonly HttpClient _identityClient;

        public InProcessIdentityServerTokenService(HttpClient identityClient)
        {
            _identityClient = identityClient;
        }

        public async Task<TokenServerResponse?> RequestPasswordTokenAsync(
            string email,
            string password,
            string? tenantId,
            CancellationToken ct)
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

            if (!response.IsSuccessStatusCode)
                return null;

            return JsonSerializer.Deserialize<TokenServerResponse>(body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<TokenServerResponse?> RefreshTokenAsync(
            string refreshToken,
            string? tenantId,
            CancellationToken ct)
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken,
                ["client_id"] = "customer-management-swagger"
            });

            var response = await _identityClient.PostAsync("connect/token", content, ct);
            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
                return null;

            return JsonSerializer.Deserialize<TokenServerResponse>(body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}
