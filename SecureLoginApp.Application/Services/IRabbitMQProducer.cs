using SecureLoginApp.Application.Models;

namespace SecureLoginApp.Application.Services;

public interface IRabbitMQProducer
{
    void SendMessage(OrderCreatedDto message);
}
