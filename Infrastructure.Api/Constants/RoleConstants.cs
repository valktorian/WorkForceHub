namespace Infrastructure.Api.Constants;

public static class RoleConstants
{
    public const string Employee = "Employee";
    public const string Manager = "Manager";
    public const string HrManager = "HRManager";
    public const string HrAdmin = "HRAdmin";

    public const string AccountAdmins = HrAdmin;
    public const string HrStaff = HrAdmin + "," + HrManager;
    public const string EvolutionUsers = HrStaff + "," + Manager;
    public const string TimeAdmins = HrAdmin;
    public const string TimeReviewers = Manager + "," + TimeAdmins;
    public const string TimeUsers = Employee + "," + TimeReviewers;
    public const string TimeEntryDeleters = Employee + "," + TimeAdmins;
    public const string EmployeeOrHrAdmin = "Employee,HRAdmin";
    public const string ManagerOrHrAdmin = "Manager,HRAdmin";
    public const string EmployeeManagerOrHrAdmin = "Employee,Manager,HRAdmin";
}
