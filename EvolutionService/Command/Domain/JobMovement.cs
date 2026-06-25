using Infrastructure.Api.Base;

namespace EvolutionService.Command.Domain;

public class JobMovement : BaseEntity
{
    public Guid EmployeeId { get; private set; }
    public string PreviousJobTitle { get; private set; } = string.Empty;
    public string NewJobTitle { get; private set; } = string.Empty;
    public string PreviousDepartment { get; private set; } = string.Empty;
    public string NewDepartment { get; private set; } = string.Empty;
    public DateTime EffectiveDate { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public string? Comment { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private JobMovement()
    {
    }

    public static JobMovement Create(
        Guid employeeId,
        string previousJobTitle,
        string newJobTitle,
        string previousDepartment,
        string newDepartment,
        DateTime effectiveDate,
        string reason,
        string? comment)
    {
        var now = DateTime.UtcNow;

        return new JobMovement
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            PreviousJobTitle = previousJobTitle.Trim(),
            NewJobTitle = newJobTitle.Trim(),
            PreviousDepartment = previousDepartment.Trim(),
            NewDepartment = newDepartment.Trim(),
            EffectiveDate = DateTime.SpecifyKind(effectiveDate, DateTimeKind.Utc),
            Reason = reason.Trim(),
            Comment = Normalize(comment),
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Update(
        string previousJobTitle,
        string newJobTitle,
        string previousDepartment,
        string newDepartment,
        DateTime effectiveDate,
        string reason,
        string? comment)
    {
        PreviousJobTitle = previousJobTitle.Trim();
        NewJobTitle = newJobTitle.Trim();
        PreviousDepartment = previousDepartment.Trim();
        NewDepartment = newDepartment.Trim();
        EffectiveDate = DateTime.SpecifyKind(effectiveDate, DateTimeKind.Utc);
        Reason = reason.Trim();
        Comment = Normalize(comment);
        UpdatedAt = DateTime.UtcNow;
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
