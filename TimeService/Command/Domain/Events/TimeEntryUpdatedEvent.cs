using Infrastructure.Api.Base;

namespace TimeService.Command.Domain.Events;

public sealed class TimeEntryUpdatedEvent : BaseEvent
{
    public Guid TimeEntryId { get; init; }
    public Guid AccountId { get; init; }
    public Guid EmployeeId { get; init; }
    public DateTime WorkDate { get; init; }
    public string StartTime { get; init; } = string.Empty;
    public string EndTime { get; init; } = string.Empty;
    public decimal Hours { get; init; }
    public string ProjectCode { get; init; } = string.Empty;
    public string TaskCode { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
