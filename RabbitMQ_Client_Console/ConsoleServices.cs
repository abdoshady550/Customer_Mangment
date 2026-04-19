using Microsoft.AspNetCore.SignalR.Client;
using RabbitMQ_Client_Console.DTOs;
using System.Text.Json;

namespace RabbitMQ_Client_Console
{

    // Hub connection factory 

    public static class HubConnectionFactory
    {
        public static string BuildHubUrl(string apiBaseUrl, string hubPath = "/hubs/queue-monitor")
        {
            if (string.IsNullOrWhiteSpace(apiBaseUrl))
                throw new ArgumentException("API base URL cannot be empty.", nameof(apiBaseUrl));

            return $"{apiBaseUrl.TrimEnd('/')}{hubPath}";
        }

        public static HubConnection Build(string hubUrl, string accessToken)
        {
            if (string.IsNullOrWhiteSpace(hubUrl))
                throw new ArgumentException("Hub URL cannot be empty.", nameof(hubUrl));

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new ArgumentException("Access token cannot be empty.", nameof(accessToken));

            return new HubConnectionBuilder()
                .WithUrl(hubUrl, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult<string?>(accessToken);
                    options.Transports =
                        Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets;
                })
                .WithAutomaticReconnect(new[]
                {
                    TimeSpan.Zero,
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(10)
                })
                .Build();
        }
    }

    // Notification parser 

    public static class NotificationParser
    {
        private static readonly JsonSerializerOptions _json =
            new() { PropertyNameCaseInsensitive = true };

        public static QueueNotification? TryParse(object? raw)
        {
            if (raw is null) return null;

            if (raw is QueueNotification typed) return typed;

            var json = raw.ToString();
            if (string.IsNullOrWhiteSpace(json)) return null;

            try
            {
                return JsonSerializer.Deserialize<QueueNotification>(json, _json);
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}
