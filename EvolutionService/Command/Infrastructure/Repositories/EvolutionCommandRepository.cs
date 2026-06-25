using EvolutionService.Command.Application.Abstractions;
using Infrastructure.Api.Base;
using Infrastructure.Api.Messaging;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EvolutionService.Command.Infrastructure.Repositories;

public class EvolutionCommandRepository<TEntity, TDbContext> : IEvolutionCommandRepository<TEntity>
    where TEntity : BaseEntity
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;

    public EvolutionCommandRepository(TDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(TEntity entity, BaseEvent evt, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
        await AddOutboxAsync(entity.Id, evt, evt, cancellationToken);
    }

    public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.Set<TEntity>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task UpdateAsync(TEntity entity, BaseEvent evt, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<TEntity>().Update(entity);
        await AddOutboxAsync(entity.Id, evt, evt, cancellationToken);
    }

    public async Task DeleteAsync(TEntity entity, BaseEvent evt, object payload, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<TEntity>().Remove(entity);
        await AddOutboxAsync(entity.Id, evt, payload, cancellationToken);
    }

    private Task AddOutboxAsync(Guid aggregateId, BaseEvent evt, object payload, CancellationToken cancellationToken)
    {
        return _dbContext.Set<OutboxMessage>().AddAsync(new OutboxMessage
        {
            AggregateType = typeof(TEntity).Name,
            AggregateId = aggregateId,
            EventType = evt.GetType().AssemblyQualifiedName!,
            Payload = JsonSerializer.Serialize(payload),
            OccurredAt = evt.OccurredAt
        }, cancellationToken).AsTask();
    }
}
