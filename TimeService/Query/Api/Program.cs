using Infrastructure.Api.Authentication;
using Infrastructure.Api.Constants;
using Infrastructure.Api.HealthChecks;
using Infrastructure.Api.Messaging;
using Infrastructure.Api.Middleware;
using Infrastructure.Api.Observability;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TimeService.Query.Application.Consumers;
using TimeService.Query.Domain.Repositories;
using TimeService.Query.Infrastructure;
using TimeService.Query.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddWorkForceHubJwtAuthentication(builder.Configuration);
builder.Services.AddWorkForceHubSwagger("WorkForceHub Time Query API");
builder.Services.AddWorkForceHubTracing(builder.Configuration, "TimeService.Query");
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
builder.Services.AddScoped<TimeEventConsumer>();
builder.Services.AddScoped<ITimeEntryReadRepository, TimeEntryReadRepository>();
builder.Services.AddScoped<ITimesheetReadRepository, TimesheetReadRepository>();
builder.Services.AddScoped<ILeaveRequestReadRepository, LeaveRequestReadRepository>();
builder.Services.AddScoped<ILeaveBalanceReadRepository, LeaveBalanceReadRepository>();
builder.Services.AddScoped<IReferenceDataRepository, ReferenceDataRepository>();

var kafkaSection = builder.Configuration.GetSection("Kafka");
var bootstrap = kafkaSection["BootstrapServers"] ?? "localhost:29092";
var topic = kafkaSection["Topic"] ?? "time.events";
var group = kafkaSection["GroupId"] ?? "time-query-group";

builder.Services.AddHostedService(sp =>
{
    var logger = sp.GetRequiredService<ILogger<KafkaConsumer>>();
    var consumer = new KafkaConsumer(bootstrap, topic, group, logger);

    foreach (var eventType in EventTypeConstants.Time.QuerySubscriptions)
    {
        consumer.RegisterHandler(eventType, async payload =>
        {
            using var scope = sp.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<TimeEventConsumer>();
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
