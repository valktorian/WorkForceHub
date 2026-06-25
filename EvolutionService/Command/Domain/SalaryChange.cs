using Infrastructure.Api.Base;

namespace EvolutionService.Command.Domain;

public class SalaryChange : BaseEntity
{
    public Guid EmployeeId { get; private set; }
    public decimal PreviousSalary { get; private set; }
    public decimal NewSalary { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public DateTime EffectiveDate { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public string? Comment { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private SalaryChange()
    {
    }

    public static SalaryChange Create(
        Guid employeeId,
        decimal previousSalary,
        decimal newSalary,
        string currency,
        DateTime effectiveDate,
        string reason,
        string? comment)
    {
        var now = DateTime.UtcNow;

        return new SalaryChange
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            PreviousSalary = previousSalary,
            NewSalary = newSalary,
            Currency = currency.Trim().ToUpperInvariant(),
            EffectiveDate = DateTime.SpecifyKind(effectiveDate, DateTimeKind.Utc),
            Reason = reason.Trim(),
            Comment = Normalize(comment),
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Update(
        decimal previousSalary,
        decimal newSalary,
        string currency,
        DateTime effectiveDate,
        string reason,
        string? comment)
    {
        PreviousSalary = previousSalary;
        NewSalary = newSalary;
        Currency = currency.Trim().ToUpperInvariant();
        EffectiveDate = DateTime.SpecifyKind(effectiveDate, DateTimeKind.Utc);
        Reason = reason.Trim();
        Comment = Normalize(comment);
        UpdatedAt = DateTime.UtcNow;
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
