using Infrastructure.Api.Base;

namespace TimeService.Command.Domain.Events;

public sealed class TimeEntryDeletedEvent : BaseEvent
{
    public Guid TimeEntryId { get; init; }
    public DateTime DeletedAt { get; init; }
}
