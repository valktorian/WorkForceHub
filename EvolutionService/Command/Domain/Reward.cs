using Infrastructure.Api.Base;

namespace EvolutionService.Command.Domain;

public class Reward : BaseEntity
{
    public Guid EmployeeId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;
    public decimal Value { get; private set; }
    public DateTime GrantedAt { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public string? Comment { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Reward()
    {
    }

    public static Reward Create(
        Guid employeeId,
        string title,
        string type,
        decimal value,
        DateTime grantedAt,
        string reason,
        string? comment)
    {
        var now = DateTime.UtcNow;

        return new Reward
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            Title = title.Trim(),
            Type = type.Trim(),
            Value = value,
            GrantedAt = DateTime.SpecifyKind(grantedAt, DateTimeKind.Utc),
            Reason = reason.Trim(),
            Comment = Normalize(comment),
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Update(
        string title,
        string type,
        decimal value,
        DateTime grantedAt,
        string reason,
        string? comment)
    {
        Title = title.Trim();
        Type = type.Trim();
        Value = value;
        GrantedAt = DateTime.SpecifyKind(grantedAt, DateTimeKind.Utc);
        Reason = reason.Trim();
        Comment = Normalize(comment);
        UpdatedAt = DateTime.UtcNow;
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
