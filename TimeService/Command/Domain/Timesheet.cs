using Infrastructure.Api.Base;

namespace TimeService.Command.Domain;

public class Timesheet : BaseEntity
{
    public Guid AccountId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public DateTime PeriodStart { get; private set; }
    public DateTime PeriodEnd { get; private set; }
    public decimal TotalHours { get; private set; }
    public string Status { get; private set; } = "Draft";
    public DateTime? SubmittedAt { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public string? ReviewComment { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Timesheet()
    {
    }

    public static Timesheet Create(Guid accountId, Guid employeeId, DateOnly periodStart, DateOnly periodEnd)
    {
        if (periodEnd < periodStart)
        {
            throw new ArgumentException("Timesheet period end must not precede its start.");
        }

        var now = DateTime.UtcNow;
        return new Timesheet
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            EmployeeId = employeeId,
            PeriodStart = periodStart.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            PeriodEnd = periodEnd.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            Status = "Draft",
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Submit()
    {
        RequireStatus("Draft");
        Status = "Submitted";
        SubmittedAt = UpdatedAt = DateTime.UtcNow;
    }

    public void Approve(string? comment)
    {
        RequireStatus("Submitted");
        Status = "Approved";
        ApprovedAt = UpdatedAt = DateTime.UtcNow;
        ReviewComment = Normalize(comment);
    }

    public void Reject(string? comment)
    {
        RequireStatus("Submitted");
        Status = "Rejected";
        UpdatedAt = DateTime.UtcNow;
        ReviewComment = Normalize(comment);
    }

    public void Reopen(string? comment)
    {
        if (string.Equals(Status, "Draft", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Timesheet is already a draft.");
        }

        Status = "Draft";
        SubmittedAt = null;
        ApprovedAt = null;
        UpdatedAt = DateTime.UtcNow;
        ReviewComment = Normalize(comment);
    }

    private void RequireStatus(string expected)
    {
        if (!string.Equals(Status, expected, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Timesheet must be {expected}.");
        }
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
