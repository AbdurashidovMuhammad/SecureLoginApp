using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SecureLoginApp.Application.Models;
using System.Text;
using System.Text.Json;

namespace SecureLoginApp.Application.Services.Impl;

public class RabbitMQProducer : IRabbitMQProducer, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMQProducer> _logger;
    private IConnection _connection;
    private IModel _channel;
    private readonly string _queueName;
    private readonly object _lock = new object();

    public RabbitMQProducer(IConfiguration configuration, ILogger<RabbitMQProducer> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _queueName = _configuration["RabbitMQ:QueueName"] ?? "orders_queue";
        _logger.LogInformation("RabbitMQ Producer yaratildi");
    }

    private void EnsureConnection()
    {
        lock (_lock)
        {
            if (_connection == null || !_connection.IsOpen)
            {
                _channel?.Dispose();
                _connection?.Dispose();

                try
                {
                    var factory = new ConnectionFactory()
                    {
                        HostName = _configuration["RabbitMQ:HostName"] ?? "localhost",
                        UserName = _configuration["RabbitMQ:UserName"] ?? "guest",
                        Password = _configuration["RabbitMQ:Password"] ?? "guest",
                        Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
                        RequestedHeartbeat = TimeSpan.FromSeconds(10),
                        NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                        AutomaticRecoveryEnabled = true
                    };

                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();

                    _channel.QueueDeclare(
                        queue: _queueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);

                    _logger.LogInformation("RabbitMQ Producer muvaffaqiyatli ulandi,  Ishlatilayotgan Queue nomi: {QueueName}", _queueName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "RabbitMQ Producer ulanishda xatolik: {Message}", ex.Message);
                    throw;
                }
            }
        }
    }

    // <<<< O'zgarish shu yerda! T emas, OrderCreatedDto qabul qilamiz
    public void SendMessage(OrderCreatedDto message) // <-- Metod nomi ham aniqroq bo'ldi
    {
        EnsureConnection();

        try
        {
            if (_channel == null || _channel.IsClosed)
            {
                _logger.LogError("RabbitMQ channel yopiq yoki mavjud emas, xabar yuborilmadi.");
                throw new InvalidOperationException("RabbitMQ channel is not open or available.");
            }

            string json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;

            _channel.BasicPublish(
                exchange: "",
                routingKey: _queueName,
                basicProperties: properties,
                body: body);

            _logger.LogInformation("Xabar RabbitMQ ga yuborildi: {Message}", json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RabbitMQ ga xabar yuborishda xatolik: {Message}", ex.Message);
            throw;
        }
    }

    // Agar sizga hali ham generik SendMessage kerak bo'lsa, uni saqlab qolishingiz mumkin,
    // lekin u boshqa DTOlar uchun ishlaydi, Order uchun emas.
    // public void SendMessage<T>(T message) {...}

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        try
        {
            _channel?.Close();
            _connection?.Close();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RabbitMQ Producer dispose qilishda xatolik");
        }
        finally
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}