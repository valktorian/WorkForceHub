using Infrastructure.Api.Base;

namespace EvolutionService.Command.Application.Abstractions;

public interface IEvolutionCommandRepository<TEntity>
    where TEntity : BaseEntity
{
    Task AddAsync(TEntity entity, BaseEvent evt, CancellationToken cancellationToken = default);

    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task UpdateAsync(TEntity entity, BaseEvent evt, CancellationToken cancellationToken = default);

    Task DeleteAsync(TEntity entity, BaseEvent evt, object payload, CancellationToken cancellationToken = default);
}
