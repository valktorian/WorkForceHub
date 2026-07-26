using Infrastructure.Api.Base;

namespace TimeService.Command.Domain.Events;

public sealed class LeaveRequestUpdatedEvent : BaseEvent
{
    public Guid LeaveRequestId { get; init; }
    public Guid AccountId { get; init; }
    public Guid EmployeeId { get; init; }
    public string LeaveType { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? Reason { get; init; }
    public DateTime? SubmittedAt { get; init; }
    public DateTime? DecisionAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
