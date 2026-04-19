using Customer_Mangment;
using Customer_Mangment.CQRS.Identity.Dto;
using Customer_Mangment.Repository.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Customer_Mangment_Integrate.Test.Common
{
    /// <summary>
    /// A custom <see cref="WebApplicationFactory{TEntryPoint}"/> that:
    /// <list type="bullet">
    ///   <item>Spins up the main Customer Management API in-process.</item>
    ///   <item>Replaces <see cref="IIdentityServerTokenService"/> with
    ///         <see cref="InProcessIdentityServerTokenService"/>, which forwards
    ///         token requests to the <b>IdentityServer application</b> hosted
    ///         by a second <see cref="WebApplicationFactory{T}"/>.</item>
    /// </list>
    /// </summary>
    public class CustomWebApplicationFactory
        : WebApplicationFactory<IAssmblyMarker>
    {
        // ── Inner factory for the Identity Server ──────────────────────────

        private readonly WebApplicationFactory<Customer_Mangment.IdentityServer.IMarkerIdentity>
            _identityFactory;

        public CustomWebApplicationFactory()
        {
            _identityFactory =
                new WebApplicationFactory<Customer_Mangment.IdentityServer.IMarkerIdentity>()
                    .WithWebHostBuilder(builder =>
                    {
                        builder.UseEnvironment("Development");
                    });
        }

        // ── Override the main API's composition root ───────────────────────

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            builder.ConfigureServices(services =>
            {
                // Replace the real IIdentityServerTokenService (which calls an
                // external process) with one that uses the in-process identity
                // server test client.
                services.RemoveAll<IIdentityServerTokenService>();
                services.AddSingleton<IIdentityServerTokenService>(
                    _ => new InProcessIdentityServerTokenService(
                        _identityFactory.CreateClient()));
            });
        }

        // ── Expose the identity server's test client ───────────────────────

        /// <summary>
        /// Returns an <see cref="HttpClient"/> that points directly at the
        /// in-process Identity Server.  Use this to acquire / refresh tokens
        /// without going through the main API.
        /// </summary>
        public HttpClient CreateIdentityClient() => _identityFactory.CreateClient();

        // ── Dispose both factories ─────────────────────────────────────────

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _identityFactory.Dispose();

            base.Dispose(disposing);
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Token service that delegates to the in-process identity server
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Implements <see cref="IIdentityServerTokenService"/> by calling the
    /// OpenIddict token endpoint on the <b>in-process</b> identity server test
    /// client.  Mirrors the logic in the real
    /// <c>IdentityServerTokenService</c>, but targets the in-memory test host
    /// instead of an external URL.
    /// </summary>
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
                    ["username"]   = email,
                    ["password"]   = password,
                    ["scope"]      = "customer_api offline_access roles",
                    ["client_id"]  = "customer-management-swagger"
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
                ["grant_type"]    = "refresh_token",
                ["refresh_token"] = refreshToken,
                ["client_id"]     = "customer-management-swagger"
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
