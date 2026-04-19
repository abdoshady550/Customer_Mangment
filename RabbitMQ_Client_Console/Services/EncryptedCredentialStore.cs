using Microsoft.AspNetCore.DataProtection;
using RabbitMQ_Client_Console.DTOs;
using RabbitMQ_Client_Console.Interfaces;
using System.Text.Json;

namespace RabbitMQ_Client_Console.Services
{
    public sealed class EncryptedCredentialStore : ICredentialStore
    {
        private readonly string _filePath;
        private readonly IDataProtector _protector;

        public EncryptedCredentialStore(string filePath, IDataProtector protector)
        {
            _filePath = filePath;
            _protector = protector;
        }

        public bool Exists => File.Exists(_filePath);

        public async Task<(string email, string password)> LoadAsync()
        {
            var json = await File.ReadAllTextAsync(_filePath);
            var credentials = JsonSerializer.Deserialize<Credentials>(json)
                              ?? throw new InvalidDataException("Credentials file is empty or malformed.");

            if (string.IsNullOrWhiteSpace(credentials.Email) || string.IsNullOrWhiteSpace(credentials.Password))
                throw new InvalidDataException("Credentials file contains blank fields.");

            return (_protector.Unprotect(credentials.Email),
                    _protector.Unprotect(credentials.Password));
        }

        public async Task SaveAsync(string email, string password)
        {
            var credentials = new Credentials
            {
                Email = _protector.Protect(email),
                Password = _protector.Protect(password)
            };

            var json = JsonSerializer.Serialize(credentials, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_filePath, json);
        }
    }
}
