using RabbitMQ_Client_Console.DTOs;

namespace RabbitMQ_Client_Console.Interfaces
{
    public interface INotificationRenderer
    {
        void Render(QueueNotification notification);
    }
}
