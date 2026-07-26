using Infrastructure.Api.Common;

namespace Infrastructure.Api.Constants;

public static class LeaveTypeConstants
{
    public const string Annual = "Annual";
    public const string Holiday = "Holiday";
    public const string Sick = "Sick";
    public const string Unpaid = "Unpaid";
    public const string Maternity = "Maternity";
    public const string Paternity = "Paternity";
    public const string Parental = "Parental";
    public const string Bereavement = "Bereavement";
    public const string Personal = "Personal";
    public const string Compensatory = "Compensatory";

    public static readonly IReadOnlyList<string> All =
    [
        Annual,
        Holiday,
        Sick,
        Unpaid,
        Maternity,
        Paternity,
        Parental,
        Bereavement,
        Personal,
        Compensatory
    ];

    public static string Normalize(string? value)
    {
        var match = All.FirstOrDefault(
            leaveType => string.Equals(leaveType, value?.Trim(), StringComparison.OrdinalIgnoreCase));

        return match ?? throw ApiException.BadRequest(
            $"Invalid leave type. Allowed values: {string.Join(", ", All)}.");
    }
}
