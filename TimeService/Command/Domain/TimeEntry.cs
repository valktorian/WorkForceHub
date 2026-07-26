using Infrastructure.Api.Base;

namespace TimeService.Command.Domain;

public class TimeEntry : BaseEntity
{
    public Guid AccountId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public DateTime WorkDate { get; private set; }
    public string StartTime { get; private set; } = string.Empty;
    public string EndTime { get; private set; } = string.Empty;
    public decimal Hours { get; private set; }
    public string ProjectCode { get; private set; } = string.Empty;
    public string TaskCode { get; private set; } = string.Empty;
    public string? Notes { get; private set; }
    public string Status { get; private set; } = "Draft";
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private TimeEntry()
    {
    }

    public static TimeEntry Create(
        Guid accountId,
        Guid employeeId,
        DateOnly workDate,
        TimeOnly startTime,
        TimeOnly endTime,
        string projectCode,
        string taskCode,
        string? notes)
    {
        ValidateTimes(startTime, endTime);
        var now = DateTime.UtcNow;

        return new TimeEntry
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            EmployeeId = employeeId,
            WorkDate = workDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            StartTime = startTime.ToString("HH:mm"),
            EndTime = endTime.ToString("HH:mm"),
            Hours = CalculateHours(startTime, endTime),
            ProjectCode = Required(projectCode, nameof(projectCode)),
            TaskCode = Required(taskCode, nameof(taskCode)),
            Notes = Optional(notes),
            Status = "Draft",
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Update(
        DateOnly workDate,
        TimeOnly startTime,
        TimeOnly endTime,
        string projectCode,
        string taskCode,
        string? notes)
    {
        if (!string.Equals(Status, "Draft", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Only draft time entries can be updated.");
        }

        ValidateTimes(startTime, endTime);
        WorkDate = workDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        StartTime = startTime.ToString("HH:mm");
        EndTime = endTime.ToString("HH:mm");
        Hours = CalculateHours(startTime, endTime);
        ProjectCode = Required(projectCode, nameof(projectCode));
        TaskCode = Required(taskCode, nameof(taskCode));
        Notes = Optional(notes);
        UpdatedAt = DateTime.UtcNow;
    }

    private static decimal CalculateHours(TimeOnly startTime, TimeOnly endTime)
        => Math.Round((decimal)(endTime - startTime).TotalHours, 2, MidpointRounding.AwayFromZero);

    private static void ValidateTimes(TimeOnly startTime, TimeOnly endTime)
    {
        if (endTime <= startTime)
        {
            throw new ArgumentException("End time must be later than start time.");
        }
    }

    private static string Required(string value, string name)
        => string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException($"{name} is required.")
            : value.Trim();

    private static string? Optional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
