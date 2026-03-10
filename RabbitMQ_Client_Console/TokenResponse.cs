namespace RabbitMQ_Client_Console
{
    public sealed record TokenResponse(string AccessToken, string RefreshToken, DateTime ExpiresOnUtc);

}
