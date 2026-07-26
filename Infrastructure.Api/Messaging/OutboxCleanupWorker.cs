using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Api.Messaging;

public sealed class OutboxCleanupWorker<TDbContext> : BackgroundService
    where TDbContext : DbContext
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxCleanupWorker<TDbContext>> _logger;
    private readonly TimeSpan _interval;

    public OutboxCleanupWorker(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<OutboxCleanupWorker<TDbContext>> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        var intervalHours = configuration.GetValue<int?>("OutboxCleanup:IntervalHours") ?? 24;
        if (intervalHours <= 0)
        {
            throw new InvalidOperationException("OutboxCleanup:IntervalHours must be greater than zero.");
        }

        _interval = TimeSpan.FromHours(intervalHours);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await using var scope = _serviceProvider.CreateAsyncScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

                var deletedCount = await dbContext.Set<OutboxMessage>()
                    .Where(message => message.PublishedAt != null)
                    .ExecuteDeleteAsync(stoppingToken);

                _logger.LogInformation(
                    "Deleted {DeletedCount} published outbox messages from {DbContext}.",
                    deletedCount,
                    typeof(TDbContext).Name);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to clean published outbox messages from {DbContext}.",
                    typeof(TDbContext).Name);
            }
        }
    }
}
