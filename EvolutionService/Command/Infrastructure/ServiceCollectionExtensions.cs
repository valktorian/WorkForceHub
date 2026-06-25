using EvolutionService.Command.Application.Abstractions;
using EvolutionService.Command.Domain;
using EvolutionService.Command.Infrastructure.Messaging;
using EvolutionService.Command.Infrastructure.Persistence;
using EvolutionService.Command.Infrastructure.Repositories;
using Infrastructure.Api.Messaging;
using Infrastructure.Api.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EvolutionService.Command.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEvolutionCommandInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is missing in configuration.");

        services.AddDbContext<EvolutionCommandDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IUnitOfWork, UnitOfWork<EvolutionCommandDbContext>>();

        services.AddScoped<IEvolutionCommandRepository<JobMovement>, EvolutionCommandRepository<JobMovement, EvolutionCommandDbContext>>();
        services.AddScoped<IEvolutionCommandRepository<SalaryChange>, EvolutionCommandRepository<SalaryChange, EvolutionCommandDbContext>>();
        services.AddScoped<IEvolutionCommandRepository<Training>, EvolutionCommandRepository<Training, EvolutionCommandDbContext>>();
        services.AddScoped<IEvolutionCommandRepository<Reward>, EvolutionCommandRepository<Reward, EvolutionCommandDbContext>>();

        var bootstrapServers = configuration["Kafka:BootstrapServers"]
            ?? throw new InvalidOperationException("Kafka:BootstrapServers is missing in configuration.");

        services.AddSingleton<IKafkaProducer>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<KafkaProducer>>();
            return new KafkaProducer(logger, bootstrapServers);
        });

        services.AddHostedService<EvolutionOutboxPublisher>();
        return services;
    }
}
