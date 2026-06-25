using EvolutionService.Query.Application.Consumers;
using EvolutionService.Query.Domain;
using EvolutionService.Query.Domain.Repositories;
using EvolutionService.Query.Infrastructure;
using EvolutionService.Query.Infrastructure.Repositories;
using Infrastructure.Api.Authentication;
using Infrastructure.Api.Constants;
using Infrastructure.Api.HealthChecks;
using Infrastructure.Api.Messaging;
using Infrastructure.Api.Middleware;
using Infrastructure.Api.Observability;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddWorkForceHubJwtAuthentication(builder.Configuration);
builder.Services.AddWorkForceHubSwagger("WorkForceHub Evolution Query API");
builder.Services.AddWorkForceHubTracing(builder.Configuration, "EvolutionService.Query");
var mongoConnectionString = builder.Configuration.GetConnectionString("ReadDatabase") ?? "mongodb://localhost:27017";
builder.Services.AddHealthChecks()
    .AddAsyncCheck("mongodb", async (ct) =>
    {
        try
        {
            var client = new MongoClient(mongoConnectionString);
            using var cursor = await client.ListDatabaseNamesAsync(ct);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(ex.Message);
        }
    });

var readDbContext = new ReadDbContext(builder.Configuration);
builder.Services.AddSingleton(readDbContext);
builder.Services.AddScoped<EvolutionEventConsumer>();
builder.Services.AddScoped<IEvolutionReadRepository<JobMovementReadModel>, JobMovementReadRepository>();
builder.Services.AddScoped<IEvolutionReadRepository<SalaryChangeReadModel>, SalaryChangeReadRepository>();
builder.Services.AddScoped<IEvolutionReadRepository<TrainingReadModel>, TrainingReadRepository>();
builder.Services.AddScoped<IEvolutionReadRepository<RewardReadModel>, RewardReadRepository>();

var kafkaSection = builder.Configuration.GetSection("Kafka");
var bootstrap = kafkaSection["BootstrapServers"] ?? "localhost:29092";
var topic = kafkaSection["Topic"] ?? "evolution.events";
var group = kafkaSection["GroupId"] ?? "evolution-query-group";

builder.Services.AddHostedService(sp =>
{
    var logger = sp.GetRequiredService<ILogger<KafkaConsumer>>();
    var consumer = new KafkaConsumer(bootstrap, topic, group, logger);
    var eventTypes = EventTypeConstants.Evolution.QuerySubscriptions;

    foreach (var eventType in eventTypes)
    {
        consumer.RegisterHandler(eventType, async payload =>
        {
            using var scope = sp.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<EvolutionEventConsumer>();
            await handler.HandleAsync(payload);
        });
    }

    return consumer;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseGlobalErrorHandler();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health", HealthCheckExtensions.DefaultOptions);
app.MapControllers();
app.Run();
