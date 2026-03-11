using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace RabbitMQ_Client_Console
{
    internal class Program
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };
        private const string ApiBaseUrl = "https://localhost:7279";

        private const string BaseSecretsPath = @"D:\CS\TECH\c#\Meccano";

        private static readonly string CredentialsFile =
            Path.Combine(BaseSecretsPath, "secrets.json");

        private static readonly string KeysFolder =
            Path.Combine(BaseSecretsPath, "keys");

        static async Task Main(string[] args)
        {
            // Data Protection setup
            var services = new ServiceCollection();

            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(KeysFolder));

            var provider = services.BuildServiceProvider();

            var protector = provider
                .GetRequiredService<IDataProtectionProvider>()
                .CreateProtector("QueueMonitor.Secrets");

            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("========================================");
            Console.WriteLine("   Customer Management - Queue Monitor");
            Console.WriteLine("========================================");
            Console.WriteLine();

            var hubUrl = $"{ApiBaseUrl}/hubs/queue-monitor";

            Console.WriteLine($"Hub URL: {hubUrl}");
            Console.WriteLine();

            var (email, password) = await LoadOrCreateCredentials(protector);


            var token = await GetTokenAsync(email, password);

            var connection = BuildConnection(hubUrl, token);

            RegisterHandlers(connection);

            await ConnectWithRetryAsync(connection);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ Connected. Listening for queue events...");
            Console.ResetColor();

            Console.WriteLine("(Press Ctrl+C to exit)");
            Console.WriteLine();

            var cts = new CancellationTokenSource();

            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nShutting down...");
                Console.ResetColor();
                cts.Cancel();
            };

            try
            {
                await Task.Delay(Timeout.Infinite, cts.Token);
            }
            catch (OperationCanceledException) { }

            await connection.StopAsync();

            Console.WriteLine("Disconnected. Goodbye.");
        }

        // Get Token 
        private static async Task<string> GetTokenAsync(string email, string password)
        {
            using var http = new HttpClient();

            var payload = new
            {
                email,
                password
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await http.PostAsync($"{ApiBaseUrl}/api/Auth/token/generate", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseJson, _jsonOptions);

            return tokenResponse?.AccessToken
                ?? throw new Exception("Failed to get access token.");
        }
        // Connection builder
        private static HubConnection BuildConnection(string hubUrl, string? token)
        {
            return new HubConnectionBuilder()
                .WithUrl(hubUrl, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult<string?>(token);

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

        // Event handlers

        private static void RegisterHandlers(HubConnection connection)
        {
            connection.On<object>("ReceiveQueueNotification", raw =>
            {
                try
                {
                    var json = raw?.ToString() ?? "{}";
                    var notification = JsonSerializer.Deserialize<QueueNotification>(json, _jsonOptions);

                    if (notification != null)
                        RenderNotification(notification);
                }
                catch
                {
                    Console.WriteLine(raw?.ToString());
                }
            });

            connection.On<QueueNotification>("ReceiveQueueNotification", notification =>
            {
                RenderNotification(notification);
            });

            connection.Reconnecting += error =>
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($" Reconnecting... {error?.Message}");
                Console.ResetColor();
                return Task.CompletedTask;
            };

            connection.Reconnected += connectionId =>
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($" Reconnected (id={connectionId})");
                Console.ResetColor();
                return Task.CompletedTask;
            };

            connection.Closed += error =>
            {
                Console.ForegroundColor = ConsoleColor.Red;

                if (error == null)
                    Console.WriteLine(" Connection closed.");
                else
                    Console.WriteLine($" Connection closed: {error.Message}");

                Console.ResetColor();

                return Task.CompletedTask;
            };
        }
        // Load or create credentials
        private static async Task<(string email, string password)> LoadOrCreateCredentials(IDataProtector protector)
        {
            if (File.Exists(CredentialsFile))
            {
                var json = await File.ReadAllTextAsync(CredentialsFile);
                var credentials = JsonSerializer.Deserialize<Credentials>(json)!;

                var email = protector.Unprotect(credentials.Email);
                var password = protector.Unprotect(credentials.Password);

                return (email, password);
            }

            Console.Write("Admin Email: ");
            var emailInput = Console.ReadLine()!;

            Console.Write("Admin Password: ");
            var passwordInput = Console.ReadLine()!;

            var secrets = new Credentials
            {
                Email = protector.Protect(emailInput),
                Password = protector.Protect(passwordInput)
            };

            var jsonOut = JsonSerializer.Serialize(secrets, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(CredentialsFile, jsonOut);

            Console.WriteLine("\nCredentials saved securely.\n");

            return (emailInput, passwordInput);
        }
        // Render notification
        private static void RenderNotification(QueueNotification n)
        {
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine($"Status     : {n.Status}");
            Console.WriteLine($"Queue      : {n.QueueName}");
            Console.WriteLine($"Message    : {n.Message}");
            Console.WriteLine($"Body       : {n.MessageBody}");
            Console.WriteLine($"ReceivedAt : {n.ReceivedAt:yyyy-MM-dd HH:mm:ss} UTC");
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine();
        }

        // Connect with retry

        private static async Task ConnectWithRetryAsync(HubConnection connection)
        {
            var attempts = 0;

            while (true)
            {
                try
                {
                    await connection.StartAsync();
                    return;
                }
                catch (Exception ex)
                {
                    attempts++;

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Connection attempt {attempts} failed: {ex.Message}");
                    Console.ResetColor();

                    if (attempts >= 5)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Could not connect after 5 attempts. Exiting.");
                        Console.ResetColor();

                        Environment.Exit(1);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempts)));
                }
            }
        }
    }
}