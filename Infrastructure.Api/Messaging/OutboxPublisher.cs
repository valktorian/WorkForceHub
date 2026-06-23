using Infrastructure.Api.Base;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Infrastructure.Api.Messaging;

public class OutboxPublisher : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxPublisher> _logger;
    private readonly IKafkaProducer _producer;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(10);

    public OutboxPublisher(IServiceProvider serviceProvider, IKafkaProducer producer, ILogger<OutboxPublisher> logger)
    {
        _serviceProvider = serviceProvider;
        _producer = producer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<DbContext>();

                var pending = await dbContext.Set<OutboxMessage>()
                    .Where(x => x.PublishedAt == null)
                    .ToListAsync(stoppingToken);

                foreach (var message in pending)
                {
                    try
                    {
                        var eventType = Type.GetType(message.EventType)!;
                        var evt = (BaseEvent)JsonSerializer.Deserialize(message.Payload, eventType)!;

                        await _producer.ProduceAsync(evt, message.Payload, "outbox");

                        message.PublishedAt = DateTime.UtcNow;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to publish outbox message {MessageId} of type {EventType}", message.Id, message.EventType);
                    }
                }

                if (pending.Any(x => x.PublishedAt != null))
                    await dbContext.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox publisher encountered an unexpected error");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}
