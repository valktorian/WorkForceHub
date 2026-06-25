using Infrastructure.Api.Base;

namespace EvolutionService.Command.Domain.Events;

public sealed class RewardUpsertedEvent : BaseEvent
{
    public Guid RewardId { get; init; }
    public Guid EmployeeId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public decimal Value { get; init; }
    public DateTime GrantedAt { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string? Comment { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
