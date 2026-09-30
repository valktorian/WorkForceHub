using Infrastructure.Api.Common;

namespace Infrastructure.Api.Constants;

public static class RoleConstants
{
    public const string Employee = "Employee";
    public const string Manager = "Manager";
    public const string HrManager = "HRManager";
    public const string HrAdmin = "HRAdmin";
    public const string HrViewer = "HRViewer";

    public static readonly IReadOnlyList<string> All =
    [
        Employee,
        Manager,
        HrManager,
        HrAdmin,
        HrViewer
    ];

    public const string AccountAdmins = HrAdmin;
    public const string HrStaff = HrAdmin + "," + HrViewer + "," + HrManager;
    public const string EvolutionUsers = HrStaff + "," + Manager;
    public const string TimeAdmins = HrAdmin + "," + HrViewer;
    public const string TimeReviewers = Manager + "," + TimeAdmins;
    public const string TimeUsers = Employee + "," + TimeReviewers;
    public const string TimeEntryDeleters = Employee + "," + TimeAdmins;

    public static string Normalize(string? value)
    {
        var match = All.FirstOrDefault(
            role => string.Equals(role, value?.Trim(), StringComparison.OrdinalIgnoreCase));

        return match ?? throw ApiException.BadRequest(
            $"Invalid role. Allowed values: {string.Join(", ", All)}.");
    }
}
