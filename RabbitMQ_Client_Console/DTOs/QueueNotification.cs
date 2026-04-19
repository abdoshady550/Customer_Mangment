namespace RabbitMQ_Client_Console.DTOs
{
    public sealed record QueueNotification(
        string QueueName,
        string Status,
        string Message,
        string MessageBody,
        DateTime ReceivedAt);
}
