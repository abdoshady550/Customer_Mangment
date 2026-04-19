namespace RabbitMQ_Client_Console.Interfaces
{
    // ── Interfaces  ─────────────────────────────────────

    public interface ICredentialStore
    {
        Task<(string email, string password)> LoadAsync();
        Task SaveAsync(string email, string password);
        bool Exists { get; }
    }
}
