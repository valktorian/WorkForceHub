using Infrastructure.Api.Constants;
using TimeService.Query.Domain;
using TimeService.Query.Domain.Repositories;

namespace TimeService.Query.Infrastructure.Repositories;

public class ReferenceDataRepository : IReferenceDataRepository
{
    public Task<IReadOnlyList<HolidayReadModel>> GetHolidaysAsync(int year, string country, CancellationToken ct)
    {
        IReadOnlyList<HolidayReadModel> items =
        [
            new HolidayReadModel { Date = new DateTime(year, 1, 1), Name = "New Year", Country = country },
            new HolidayReadModel { Date = new DateTime(year, 5, 1), Name = "Labour Day", Country = country }
        ];

        return Task.FromResult(items);
    }

    public Task<IReadOnlyList<LeaveTypeReadModel>> GetLeaveTypesAsync(CancellationToken ct)
    {
        IReadOnlyList<LeaveTypeReadModel> items =
        [
            new LeaveTypeReadModel { Code = LeaveTypeConstants.Annual, Name = "Annual Leave", IsPaid = true },
            new LeaveTypeReadModel { Code = LeaveTypeConstants.Holiday, Name = "Holiday Leave", IsPaid = true },
            new LeaveTypeReadModel { Code = LeaveTypeConstants.Sick, Name = "Sick Leave", IsPaid = true },
            new LeaveTypeReadModel { Code = LeaveTypeConstants.Unpaid, Name = "Unpaid Leave", IsPaid = false },
            new LeaveTypeReadModel { Code = LeaveTypeConstants.Maternity, Name = "Maternity Leave", IsPaid = true },
            new LeaveTypeReadModel { Code = LeaveTypeConstants.Paternity, Name = "Paternity Leave", IsPaid = true },
            new LeaveTypeReadModel { Code = LeaveTypeConstants.Parental, Name = "Parental Leave", IsPaid = true },
            new LeaveTypeReadModel { Code = LeaveTypeConstants.Bereavement, Name = "Bereavement Leave", IsPaid = true },
            new LeaveTypeReadModel { Code = LeaveTypeConstants.Personal, Name = "Personal Leave", IsPaid = true },
            new LeaveTypeReadModel { Code = LeaveTypeConstants.Compensatory, Name = "Compensatory Leave", IsPaid = true }
        ];

        return Task.FromResult(items);
    }
}
