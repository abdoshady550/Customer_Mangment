using Customer_Mangment.Repository.Interfaces;

namespace Customer_Mangment.Repository.Services
{
    public class IdentityServerTokenService : IIdentityServerTokenService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public IdentityServerTokenService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<TokenServerResponse?> RequestPasswordTokenAsync(string email, string password, CancellationToken ct)
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["username"] = email,
                ["password"] = password,
                ["scope"] = "customer_api offline_access roles",
                ["client_id"] = "customer-management-swagger"
            });

            var response = await _httpClient.PostAsync($"{_config["Auth:Authority"]}/connect/token", content, ct);
            return await response.Content.ReadFromJsonAsync<TokenServerResponse>(cancellationToken: ct);
        }

        public async Task<TokenServerResponse?> RefreshTokenAsync(string refreshToken, CancellationToken ct)
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken,
                ["client_id"] = "customer-management-swagger"
            });

            var response = await _httpClient.PostAsync($"{_config["Auth:Authority"]}/connect/token", content, ct);
            return await response.Content.ReadFromJsonAsync<TokenServerResponse>(cancellationToken: ct);
        }
    }
}
