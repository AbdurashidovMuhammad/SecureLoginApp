// SecureLoginApp.Application.Services/RabbitMQConsumer.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using SecureLoginApp.Core.Entities; // Order entitiyasi
using SecureLoginApp.DataAcces.Persistence; // AppDbContext
using SecureLoginApp.Application.Models; // <<<< YANGI: OrderCreatedDto

namespace SecureLoginApp.Application.Services;

public class RabbitMQConsumer : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RabbitMQConsumer> _logger;
    private IConnection? _connection;
    private IModel? _channel;
    private readonly string _queueName;

    public RabbitMQConsumer(
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory,
        ILogger<RabbitMQConsumer> logger)
    {
        _configuration = configuration;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _queueName = _configuration["RabbitMQ:QueueName"] ?? "myQueue";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(2000, stoppingToken); // Application ishga tushishi uchun kutamiz

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await StartConsumer(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RabbitMQ Consumer xatolik: {Message}", ex.Message);
                await Task.Delay(5000, stoppingToken); // 5 soniya kutib qaytadan urinib ko'ramiz
            }
        }
    }

    private async Task StartConsumer(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory()
        {
            HostName = _configuration["RabbitMQ:HostName"] ?? "localhost",
            UserName = _configuration["RabbitMQ:UserName"] ?? "guest",
            Password = _configuration["RabbitMQ:Password"] ?? "guest",
            Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672")
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(
            queue: _queueName,
            durable: true, // Persistent queue
            exclusive: false,
            autoDelete: false,
            arguments: null);

        // Fair dispatch - har bir consumer ga faqat bitta message
        _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                _logger.LogInformation("Xabar qabul qilindi: {Message}", message);

                // <<<< O'zgarish shu yerda! DTO ga deserializatsiya qilamiz
                var orderDto = JsonSerializer.Deserialize<OrderCreatedDto>(message);

                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                if (orderDto != null)
                {
                    // DTO dan yangi Order entitisini yaratamiz
                    var newOrder = new Order
                    {
                        ProductName = orderDto.ProductName
                    };
                    _logger.LogInformation("Xabar qayta ishlanishidan oldin 5 soniya kutilmoqda...");
                    await Task.Delay(5000);
                    // Endi bu newOrder ning Id si default qiymatda (0) bo'ladi
                    // Baza uni avtomatik ravishda generatsiya qiladi
                    db.Orders.Add(newOrder);
                    await db.SaveChangesAsync();

                    // newOrder.Id endi bazadan kelgan Id qiymati bilan to'ldirilgan bo'ladi
                    _logger.LogInformation("Bazaga yozildi: {ProductName} (ID: {Id})", newOrder.ProductName, newOrder.Id);

                    // Manual acknowledgment
                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                else
                {
                    _logger.LogWarning("Order DTO deserializatsiya qilinmadi. Xabar tarkibi: {Message}", message);
                    // Agar deserializatsiya muvaffaqiyatsiz bo'lsa, xabarni qayta navbatga qo'ymaymiz.
                    _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RabbitMQ message ni qayta ishlashda xatolik. Xabar: {Message}", Encoding.UTF8.GetString(ea.Body.ToArray()));
                // Xatolik bo'lsa message ni qayta queue ga qo'yamiz
                // Agar xato DbUpdateException bo'lsa va Id duplikati bo'lsa,
                // requeue: true yana xato berishga olib kelishi mumkin.
                // Lekin boshqa turdagi vaqtinchalik xatolar uchun foydali.
                _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        _channel.BasicConsume(
            queue: _queueName,
            autoAck: false, // Manual acknowledgment
            consumer: consumer);

        _logger.LogInformation("RabbitMQ Consumer ishga tushdi");

        // Background service davom etishi uchun
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("RabbitMQ Consumer to'xtatilmoqda...");
        _channel?.Close();
        _connection?.Close();
        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}