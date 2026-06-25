using Infrastructure.Api.Base;

namespace EvolutionService.Command.Domain.Events;

public sealed class JobMovementDeletedEvent : BaseEvent
{
    public Guid JobMovementId { get; init; }
    public DateTime DeletedAt { get; init; }
}
