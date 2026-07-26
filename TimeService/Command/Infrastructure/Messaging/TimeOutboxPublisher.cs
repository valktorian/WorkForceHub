using Infrastructure.Api.Base;
using Infrastructure.Api.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TimeService.Command.Infrastructure.Persistence;

namespace TimeService.Command.Infrastructure.Messaging;

public sealed class TimeOutboxPublisher : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IKafkaProducer _producer;
    private readonly ILogger<TimeOutboxPublisher> _logger;
    private readonly string _topic;
    private readonly TimeSpan _interval;

    public TimeOutboxPublisher(
        IServiceProvider serviceProvider,
        IKafkaProducer producer,
        IConfiguration configuration,
        ILogger<TimeOutboxPublisher> logger)
    {
        _serviceProvider = serviceProvider;
        _producer = producer;
        _logger = logger;
        _topic = configuration["Kafka:Topic"] ?? "time.events";
        _interval = TimeSpan.FromSeconds(
            configuration.GetValue<int?>("Kafka:OutboxIntervalSeconds") ?? 5);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<TimeCommandDbContext>();
                var pendingMessages = await dbContext.OutboxMessages
                    .Where(x => x.PublishedAt == null)
                    .OrderBy(x => x.OccurredAt)
                    .Take(20)
                    .ToListAsync(stoppingToken);

                foreach (var message in pendingMessages)
                {
                    var eventType = Type.GetType(message.EventType);
                    if (eventType is null)
                    {
                        _logger.LogWarning(
                            "Unable to load event type {EventType} for outbox message {OutboxId}.",
                            message.EventType,
                            message.Id);
                        continue;
                    }

                    var evt = JsonSerializer.Deserialize(message.Payload, eventType) as BaseEvent;
                    if (evt is null)
                    {
                        _logger.LogWarning(
                            "Unable to deserialize outbox message {OutboxId}.",
                            message.Id);
                        continue;
                    }

                    var payload = JsonSerializer.Deserialize<JsonElement>(message.Payload);
                    await _producer.ProduceAsync(evt, payload, _topic);
                    message.PublishedAt = DateTime.UtcNow;
                }

                if (pendingMessages.Count > 0)
                {
                    await dbContext.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while publishing time outbox messages.");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}
