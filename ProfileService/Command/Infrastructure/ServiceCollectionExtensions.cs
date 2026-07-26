using Infrastructure.Api.Messaging;
using Infrastructure.Api.Persistence;
using Infrastructure.Api.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProfileService.Command.Application.Abstractions;
using ProfileService.Command.Infrastructure.Messaging;
using ProfileService.Command.Infrastructure.Persistence;
using ProfileService.Command.Infrastructure.Repositories;

namespace ProfileService.Command.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProfileCommandInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is missing in configuration.");

        services.AddDbContext<ProfileCommandDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IProfileRepository, ProfileRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork<ProfileCommandDbContext>>();

        var bootstrapServers = configuration["Kafka:BootstrapServers"]
            ?? throw new InvalidOperationException("Kafka:BootstrapServers is missing in configuration.");

        services.AddSingleton<IKafkaProducer>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<KafkaProducer>>();
            return new KafkaProducer(logger, bootstrapServers);
        });

        services.Configure<ExternalFileStorageOptions>(configuration.GetSection("MediaStorage"));
        services.AddHttpClient<IExternalFileStorageClient, HttpExternalFileStorageClient>((sp, client) =>
        {
            var options = configuration.GetSection("MediaStorage").Get<ExternalFileStorageOptions>()
                ?? throw new InvalidOperationException("MediaStorage configuration is missing.");

            if (string.IsNullOrWhiteSpace(options.BaseUrl))
            {
                throw new InvalidOperationException("MediaStorage:BaseUrl is missing in configuration.");
            }

            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });

        services.AddHostedService<ProfileOutboxPublisher>();
        services.AddHostedService<OutboxCleanupWorker<ProfileCommandDbContext>>();
        return services;
    }
}
