using EvolutionService.Query.Domain;
using EvolutionService.Query.Infrastructure;
using Infrastructure.Api.Constants;
using Infrastructure.Api.Mapping;
using Infrastructure.Api.Messaging;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.Text.Json;

namespace EvolutionService.Query.Application.Consumers;

public class EvolutionEventConsumer : IEventHandler
{
    private readonly ReadDbContext _readDb;
    private readonly ILogger<EvolutionEventConsumer> _logger;

    public string EventType => EventTypeConstants.Evolution.JobMovementUpserted;

    public EvolutionEventConsumer(ReadDbContext readDb, ILogger<EvolutionEventConsumer> logger)
    {
        _readDb = readDb;
        _logger = logger;
    }

    public async Task HandleAsync(JsonElement payload)
    {
        try
        {
            if (payload.TryGetProperty("JobMovementId", out _))
            {
                await HandleAsync(payload, _readDb.JobMovements, "JobMovementId");
                return;
            }

            if (payload.TryGetProperty("SalaryChangeId", out _))
            {
                await HandleAsync(payload, _readDb.SalaryChanges, "SalaryChangeId");
                return;
            }

            if (payload.TryGetProperty("TrainingId", out _))
            {
                await HandleAsync(payload, _readDb.Trainings, "TrainingId");
                return;
            }

            if (payload.TryGetProperty("RewardId", out _))
            {
                await HandleAsync(payload, _readDb.Rewards, "RewardId");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling evolution event payload.");
        }
    }

    private static async Task HandleAsync<TModel>(
        JsonElement payload,
        IMongoCollection<TModel> collection,
        string idProperty)
        where TModel : class, new()
    {
        if (payload.TryGetProperty("DeletedAt", out _))
        {
            var id = payload.GetProperty(idProperty).GetGuid();
            var filter = Builders<TModel>.Filter.Eq("_id", id);
            await collection.DeleteOneAsync(filter);
            return;
        }

        var model = JsonElementMapper.Map<TModel>(payload, (source, readModel) =>
        {
            readModel.GetType().GetProperty("Id")?.SetValue(readModel, source.GetRequiredGuid(idProperty));
        });
        var entityId = payload.GetRequiredGuid(idProperty);
        await collection.ReplaceOneAsync(Builders<TModel>.Filter.Eq("_id", entityId), model, new ReplaceOptions { IsUpsert = true });
    }
}
