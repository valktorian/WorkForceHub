using Infrastructure.Api.Base;

namespace EvolutionService.Command.Domain.Events;

public sealed class JobMovementUpsertedEvent : BaseEvent
{
    public Guid JobMovementId { get; init; }
    public Guid EmployeeId { get; init; }
    public string PreviousJobTitle { get; init; } = string.Empty;
    public string NewJobTitle { get; init; } = string.Empty;
    public string PreviousDepartment { get; init; } = string.Empty;
    public string NewDepartment { get; init; } = string.Empty;
    public DateTime EffectiveDate { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string? Comment { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
