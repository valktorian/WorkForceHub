using Infrastructure.Api.Authentication;
using Infrastructure.Api.Constants;
using Infrastructure.Api.HealthChecks;
using Infrastructure.Api.Messaging;
using Infrastructure.Api.Middleware;
using Infrastructure.Api.Observability;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ProfileService.Query.Application.Consumers;
using ProfileService.Query.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddWorkForceHubJwtAuthentication(builder.Configuration);
builder.Services.AddWorkForceHubSwagger("WorkForceHub Profile Query API");
builder.Services.AddWorkForceHubTracing(builder.Configuration, "ProfileService.Query");
var mongoConnectionString = builder.Configuration.GetConnectionString("ReadDatabase") ?? "mongodb://localhost:27017";
builder.Services.AddHealthChecks()
    .AddAsyncCheck("mongodb", async (ct) =>
    {
        try
        {
            var client = new MongoDB.Driver.MongoClient(mongoConnectionString);
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
builder.Services.AddScoped<ProfileEventConsumer>();

var kafkaSection = builder.Configuration.GetSection("Kafka");
var bootstrap = kafkaSection["BootstrapServers"] ?? "localhost:29092";
var topic = kafkaSection["Topic"] ?? "profile.events";
var group = kafkaSection["GroupId"] ?? "profile-query-group";

builder.Services.AddHostedService(sp =>
{
    var logger = sp.GetRequiredService<ILogger<KafkaConsumer>>();
    var consumer = new KafkaConsumer(bootstrap, topic, group, logger);

    foreach (var eventType in EventTypeConstants.Profile.QuerySubscriptions)
    {
        consumer.RegisterHandler(eventType, async payload =>
        {
            using var scope = sp.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<ProfileEventConsumer>();
            await handler.HandleAsync(payload);
        });
    }

    return consumer;
});

var app = builder.Build();

app.UseSwagger();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI();
}

app.UseGlobalErrorHandler();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health", HealthCheckExtensions.DefaultOptions);
app.MapControllers();

app.Run();
