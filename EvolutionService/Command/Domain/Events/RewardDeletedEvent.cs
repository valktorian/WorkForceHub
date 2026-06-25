using Infrastructure.Api.Base;

namespace EvolutionService.Command.Domain.Events;

public sealed class RewardDeletedEvent : BaseEvent
{
    public Guid RewardId { get; init; }
    public DateTime DeletedAt { get; init; }
}
