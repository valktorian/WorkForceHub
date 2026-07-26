using AccountService.Command.Application.Abstractions;
using AccountService.Command.Infrastructure.Messaging;
using AccountService.Command.Infrastructure.Persistence;
using AccountService.Command.Infrastructure.Repositories;
using Infrastructure.Api.Messaging;
using Infrastructure.Api.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AccountService.Command.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAccountCommandInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is missing in configuration.");

        services.AddDbContext<AccountCommandDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork<AccountCommandDbContext>>();

        var bootstrapServers = configuration["Kafka:BootstrapServers"]
            ?? throw new InvalidOperationException("Kafka:BootstrapServers is missing in configuration.");

        services.AddSingleton<IKafkaProducer>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<KafkaProducer>>();
            return new KafkaProducer(logger, bootstrapServers);
        });

        services.AddHostedService<AccountOutboxPublisher>();
        services.AddHostedService<OutboxCleanupWorker<AccountCommandDbContext>>();

        return services;
    }
}
