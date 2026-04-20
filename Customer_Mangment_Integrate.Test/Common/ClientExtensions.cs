using Newtonsoft.Json;

namespace Customer_Mangment_Integrate.Test.Common
{
    public partial class Client
    {
        public virtual async Task<TokenServerResponse> RequestPasswordTokenAsync(
            string userName,
            string password,
            string clientId = "customer-management-swagger",
            string scope = "customer_api offline_access roles",
            CancellationToken cancellationToken = default)
        {
            var client_ = _httpClient;
            using var request_ = new HttpRequestMessage(HttpMethod.Post, "connect/token");

            var parameters = new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["username"] = userName,
                ["password"] = password,
                ["client_id"] = clientId,
                ["scope"] = scope
            };

            request_.Content = new FormUrlEncodedContent(parameters);
            request_.Headers.Accept.ParseAdd("application/json");

            var response_ = await client_.SendAsync(request_, cancellationToken).ConfigureAwait(false);
            var headers_ = new Dictionary<string, IEnumerable<string>>();
            foreach (var h in response_.Headers) headers_[h.Key] = h.Value;
            if (response_.Content?.Headers != null)
                foreach (var h in response_.Content.Headers) headers_[h.Key] = h.Value;

            var status_ = (int)response_.StatusCode;
            var responseText = await response_.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (status_ == 200)
            {
                var tokenResponse = JsonConvert.DeserializeObject<TokenServerResponse>(responseText, JsonSerializerSettings);
                if (tokenResponse == null)
                    throw new ApiException("Response was null", status_, responseText, headers_, null);
                return tokenResponse;
            }

            throw new ApiException($"Token request failed with status {status_}", status_, responseText, headers_, null);
        }

        public virtual async Task<TokenServerResponse> RequestRefreshTokenAsync(
            string refreshToken,
            string expiredAccessToken,
            string clientId = "customer-management-swagger",
            CancellationToken cancellationToken = default)
        {
            var client_ = _httpClient;
            using var request_ = new HttpRequestMessage(HttpMethod.Post, "connect/token");

            var parameters = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken,
                ["client_id"] = clientId,
            };

            request_.Content = new FormUrlEncodedContent(parameters);
            request_.Headers.Accept.ParseAdd("application/json");

            var response_ = await client_.SendAsync(request_, cancellationToken).ConfigureAwait(false);
            var headers_ = new Dictionary<string, IEnumerable<string>>();
            foreach (var h in response_.Headers) headers_[h.Key] = h.Value;
            if (response_.Content?.Headers != null)
                foreach (var h in response_.Content.Headers) headers_[h.Key] = h.Value;

            var status_ = (int)response_.StatusCode;
            var responseText = await response_.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (status_ == 200)
            {
                var tokenResponse = JsonConvert.DeserializeObject<TokenServerResponse>(responseText, JsonSerializerSettings);
                if (tokenResponse == null)
                    throw new ApiException("Response was null", status_, responseText, headers_, null);
                return tokenResponse;
            }

            throw new ApiException($"Token refresh failed with status {status_}", status_, responseText, headers_, null);
        }
    }
}
