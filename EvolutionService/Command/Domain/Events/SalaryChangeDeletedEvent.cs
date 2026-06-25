using Infrastructure.Api.Base;

namespace EvolutionService.Command.Domain.Events;

public sealed class SalaryChangeDeletedEvent : BaseEvent
{
    public Guid SalaryChangeId { get; init; }
    public DateTime DeletedAt { get; init; }
}
