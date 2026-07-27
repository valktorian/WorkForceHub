using Infrastructure.Api.Base;
using TimeService.Command.Domain;

namespace TimeService.Command.Application.Abstractions;

public interface ITimeCommandRepository<TEntity>
    where TEntity : BaseEntity
{
    Task AddAsync(TEntity entity, BaseEvent evt, CancellationToken cancellationToken = default);
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(TEntity entity, BaseEvent evt, CancellationToken cancellationToken = default);
    Task DeleteAsync(TEntity entity, BaseEvent evt, object payload, CancellationToken cancellationToken = default);
}

public interface ILeaveBalanceRepository : ITimeCommandRepository<LeaveBalance>
{
    Task<LeaveBalance?> GetByEmployeeAndTypeAsync(
        Guid employeeId,
        string leaveType,
        CancellationToken cancellationToken = default);
}

public interface ITimesheetRepository : ITimeCommandRepository<Timesheet>
{
    Task<Timesheet?> GetByEmployeeAndPeriodAsync(
        Guid employeeId,
        DateOnly periodStart,
        DateOnly periodEnd,
        CancellationToken cancellationToken = default);
}
