namespace Customer_Mangment.Hubs.Models
{
    public sealed record QueueNotification(string QueueName,
                                           string Status,
                                           string Message,
                                           string MessageBody,
                                           DateTime ReceivedAt);
}
