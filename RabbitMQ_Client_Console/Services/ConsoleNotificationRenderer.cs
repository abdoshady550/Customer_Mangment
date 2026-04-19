using RabbitMQ_Client_Console.DTOs;
using RabbitMQ_Client_Console.Interfaces;

namespace RabbitMQ_Client_Console.Services
{
    public sealed class ConsoleNotificationRenderer : INotificationRenderer
    {
        public void Render(QueueNotification n)
        {
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine($"Status     : {n.Status}");
            Console.WriteLine($"Queue      : {n.QueueName}");
            Console.WriteLine($"Message    : {n.Message}");
            Console.WriteLine($"Body       : {n.MessageBody}");
            Console.WriteLine($"ReceivedAt : {n.ReceivedAt:yyyy-MM-dd HH:mm:ss} UTC");
            Console.WriteLine("--------------------------------------------------");
        }
    }
}
