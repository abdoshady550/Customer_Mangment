namespace RabbitMQ_Client_Console.Interfaces
{
    public interface ITokenClient
    {
        Task<string> GetAccessTokenAsync(string email, string password, CancellationToken ct = default);
    }
}
