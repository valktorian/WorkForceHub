using Infrastructure.Api.Base;

namespace EvolutionService.Command.Domain;

public class Training : BaseEntity
{
    public Guid EmployeeId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Provider { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public DateTime? CompletionDate { get; private set; }
    public string? CertificateUrl { get; private set; }
    public string? Comment { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Training()
    {
    }

    public static Training Create(
        Guid employeeId,
        string title,
        string provider,
        string status,
        DateTime startDate,
        DateTime? endDate,
        DateTime? completionDate,
        string? certificateUrl,
        string? comment)
    {
        var now = DateTime.UtcNow;

        return new Training
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            Title = title.Trim(),
            Provider = provider.Trim(),
            Status = status.Trim(),
            StartDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc),
            EndDate = ToUtc(endDate),
            CompletionDate = ToUtc(completionDate),
            CertificateUrl = Normalize(certificateUrl),
            Comment = Normalize(comment),
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Update(
        string title,
        string provider,
        string status,
        DateTime startDate,
        DateTime? endDate,
        DateTime? completionDate,
        string? certificateUrl,
        string? comment)
    {
        Title = title.Trim();
        Provider = provider.Trim();
        Status = status.Trim();
        StartDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
        EndDate = ToUtc(endDate);
        CompletionDate = ToUtc(completionDate);
        CertificateUrl = Normalize(certificateUrl);
        Comment = Normalize(comment);
        UpdatedAt = DateTime.UtcNow;
    }

    private static DateTime? ToUtc(DateTime? value) => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;
    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
