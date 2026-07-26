using Infrastructure.Api.Base;
using Infrastructure.Api.Constants;

namespace TimeService.Command.Domain;

public class LeaveBalance : BaseEntity
{
    public Guid EmployeeId { get; private set; }
    public string LeaveType { get; private set; } = string.Empty;
    public decimal Available { get; private set; }
    public decimal Used { get; private set; }
    public decimal Pending { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private LeaveBalance()
    {
    }

    public static LeaveBalance Create(Guid employeeId, string leaveType)
        => new()
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            LeaveType = LeaveTypeConstants.Normalize(leaveType),
            UpdatedAt = DateTime.UtcNow
        };

    public void Adjust(decimal delta)
    {
        if (Available + delta < 0)
        {
            throw new InvalidOperationException("Leave balance cannot become negative.");
        }

        Available += delta;
        UpdatedAt = DateTime.UtcNow;
    }

}
