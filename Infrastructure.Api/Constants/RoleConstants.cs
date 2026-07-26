namespace Infrastructure.Api.Constants;

public static class RoleConstants
{
    public const string HrAdmin = "HRAdmin";
    public const string EmployeeOrHrAdmin = "Employee,HRAdmin";
    public const string ManagerOrHrAdmin = "Manager,HRAdmin";
    public const string EmployeeManagerOrHrAdmin = "Employee,Manager,HRAdmin";
}
