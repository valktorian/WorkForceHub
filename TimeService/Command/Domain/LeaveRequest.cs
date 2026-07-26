using Infrastructure.Api.Base;
using Infrastructure.Api.Constants;

namespace TimeService.Command.Domain;

public class LeaveRequest : BaseEntity
{
    public Guid AccountId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public string LeaveType { get; private set; } = string.Empty;
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public string Status { get; private set; } = "Draft";
    public string? Reason { get; private set; }
    public string? DecisionComment { get; private set; }
    public DateTime? SubmittedAt { get; private set; }
    public DateTime? DecisionAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private LeaveRequest()
    {
    }

    public static LeaveRequest Create(
        Guid accountId,
        Guid employeeId,
        string leaveType,
        DateOnly startDate,
        DateOnly endDate,
        string? reason)
    {
        ValidateDates(startDate, endDate);
        var now = DateTime.UtcNow;

        return new LeaveRequest
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            EmployeeId = employeeId,
            LeaveType = LeaveTypeConstants.Normalize(leaveType),
            StartDate = startDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            EndDate = endDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            Status = "Draft",
            Reason = Normalize(reason),
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Update(string leaveType, DateOnly startDate, DateOnly endDate, string? reason)
    {
        RequireStatus("Draft");
        ValidateDates(startDate, endDate);
        LeaveType = LeaveTypeConstants.Normalize(leaveType);
        StartDate = startDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        EndDate = endDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        Reason = Normalize(reason);
        UpdatedAt = DateTime.UtcNow;
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
        DecisionAt = UpdatedAt = DateTime.UtcNow;
        DecisionComment = Normalize(comment);
    }

    public void Reject(string? comment)
    {
        RequireStatus("Submitted");
        Status = "Rejected";
        DecisionAt = UpdatedAt = DateTime.UtcNow;
        DecisionComment = Normalize(comment);
    }

    public void Cancel(string? comment)
    {
        if (string.Equals(Status, "Approved", StringComparison.OrdinalIgnoreCase)
            || string.Equals(Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Approved or already-cancelled leave requests cannot be cancelled.");
        }

        Status = "Cancelled";
        DecisionAt = UpdatedAt = DateTime.UtcNow;
        DecisionComment = Normalize(comment);
    }

    private void RequireStatus(string expected)
    {
        if (!string.Equals(Status, expected, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Leave request must be {expected}.");
        }
    }

    private static void ValidateDates(DateOnly startDate, DateOnly endDate)
    {
        if (endDate < startDate)
        {
            throw new ArgumentException("Leave end date must not precede its start date.");
        }
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
