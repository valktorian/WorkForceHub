using Infrastructure.Api.Base;

namespace EvolutionService.Command.Domain.Events;

public sealed class SalaryChangeUpsertedEvent : BaseEvent
{
    public Guid SalaryChangeId { get; init; }
    public Guid EmployeeId { get; init; }
    public decimal PreviousSalary { get; init; }
    public decimal NewSalary { get; init; }
    public string Currency { get; init; } = string.Empty;
    public DateTime EffectiveDate { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string? Comment { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
