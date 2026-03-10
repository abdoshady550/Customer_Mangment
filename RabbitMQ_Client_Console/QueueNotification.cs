namespace RabbitMQ_Client_Console
{
    public sealed record QueueNotification(
        string QueueName,
        string Status,
        string Message,
        string MessageBody,
        DateTime ReceivedAt);

}
