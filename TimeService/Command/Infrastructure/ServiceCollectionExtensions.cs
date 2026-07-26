using Infrastructure.Api.Messaging;
using Infrastructure.Api.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TimeService.Command.Application.Abstractions;
using TimeService.Command.Domain;
using TimeService.Command.Infrastructure.Messaging;
using TimeService.Command.Infrastructure.Persistence;
using TimeService.Command.Infrastructure.Repositories;

namespace TimeService.Command.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTimeCommandInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is missing in configuration.");

        services.AddDbContext<TimeCommandDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IUnitOfWork, UnitOfWork<TimeCommandDbContext>>();
        services.AddScoped<ITimeCommandRepository<TimeEntry>, TimeCommandRepository<TimeEntry>>();
        services.AddScoped<ITimeCommandRepository<Timesheet>, TimeCommandRepository<Timesheet>>();
        services.AddScoped<ITimeCommandRepository<LeaveRequest>, TimeCommandRepository<LeaveRequest>>();
        services.AddScoped<ILeaveBalanceRepository, LeaveBalanceRepository>();

        var bootstrapServers = configuration["Kafka:BootstrapServers"]
            ?? throw new InvalidOperationException("Kafka:BootstrapServers is missing in configuration.");

        services.AddSingleton<IKafkaProducer>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<KafkaProducer>>();
            return new KafkaProducer(logger, bootstrapServers);
        });

        services.AddHostedService<TimeOutboxPublisher>();
        services.AddHostedService<OutboxCleanupWorker<TimeCommandDbContext>>();
        return services;
    }
}
