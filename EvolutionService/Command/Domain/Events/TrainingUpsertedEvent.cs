using Infrastructure.Api.Base;

namespace EvolutionService.Command.Domain.Events;

public sealed class TrainingUpsertedEvent : BaseEvent
{
    public Guid TrainingId { get; init; }
    public Guid EmployeeId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public DateTime? CompletionDate { get; init; }
    public string? CertificateUrl { get; init; }
    public string? Comment { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
