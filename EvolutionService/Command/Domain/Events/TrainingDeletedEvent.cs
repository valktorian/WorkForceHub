using Infrastructure.Api.Base;

namespace EvolutionService.Command.Domain.Events;

public sealed class TrainingDeletedEvent : BaseEvent
{
    public Guid TrainingId { get; init; }
    public DateTime DeletedAt { get; init; }
}
