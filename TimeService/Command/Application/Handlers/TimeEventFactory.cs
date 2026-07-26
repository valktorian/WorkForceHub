using TimeService.Command.Domain;
using TimeService.Command.Domain.Events;

namespace TimeService.Command.Application.Handlers;

internal static class TimeEventFactory
{
    internal static TimeEntryCreatedEvent Created(TimeEntry entity) => new()
    {
        TimeEntryId = entity.Id,
        AccountId = entity.AccountId,
        EmployeeId = entity.EmployeeId,
        WorkDate = entity.WorkDate,
        StartTime = entity.StartTime,
        EndTime = entity.EndTime,
        Hours = entity.Hours,
        ProjectCode = entity.ProjectCode,
        TaskCode = entity.TaskCode,
        Notes = entity.Notes,
        Status = entity.Status,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };

    internal static TimeEntryUpdatedEvent Updated(TimeEntry entity) => new()
    {
        TimeEntryId = entity.Id,
        AccountId = entity.AccountId,
        EmployeeId = entity.EmployeeId,
        WorkDate = entity.WorkDate,
        StartTime = entity.StartTime,
        EndTime = entity.EndTime,
        Hours = entity.Hours,
        ProjectCode = entity.ProjectCode,
        TaskCode = entity.TaskCode,
        Notes = entity.Notes,
        Status = entity.Status,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };

    internal static TimesheetCreatedEvent Created(Timesheet entity) => new()
    {
        TimesheetId = entity.Id,
        AccountId = entity.AccountId,
        EmployeeId = entity.EmployeeId,
        PeriodStart = entity.PeriodStart,
        PeriodEnd = entity.PeriodEnd,
        TotalHours = entity.TotalHours,
        Status = entity.Status,
        SubmittedAt = entity.SubmittedAt,
        ApprovedAt = entity.ApprovedAt,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };

    internal static TimesheetStatusChangedEvent Status(Timesheet entity) => new()
    {
        TimesheetId = entity.Id,
        Status = entity.Status,
        SubmittedAt = entity.SubmittedAt,
        ApprovedAt = entity.ApprovedAt,
        UpdatedAt = entity.UpdatedAt,
        Comment = entity.ReviewComment
    };

    internal static LeaveRequestCreatedEvent Created(LeaveRequest entity) => new()
    {
        LeaveRequestId = entity.Id,
        AccountId = entity.AccountId,
        EmployeeId = entity.EmployeeId,
        LeaveType = entity.LeaveType,
        StartDate = entity.StartDate,
        EndDate = entity.EndDate,
        Status = entity.Status,
        Reason = entity.Reason,
        SubmittedAt = entity.SubmittedAt,
        DecisionAt = entity.DecisionAt,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };

    internal static LeaveRequestUpdatedEvent Updated(LeaveRequest entity) => new()
    {
        LeaveRequestId = entity.Id,
        AccountId = entity.AccountId,
        EmployeeId = entity.EmployeeId,
        LeaveType = entity.LeaveType,
        StartDate = entity.StartDate,
        EndDate = entity.EndDate,
        Status = entity.Status,
        Reason = entity.Reason,
        SubmittedAt = entity.SubmittedAt,
        DecisionAt = entity.DecisionAt,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };

    internal static LeaveRequestStatusChangedEvent Status(LeaveRequest entity) => new()
    {
        LeaveRequestId = entity.Id,
        Status = entity.Status,
        SubmittedAt = entity.SubmittedAt,
        DecisionAt = entity.DecisionAt,
        UpdatedAt = entity.UpdatedAt,
        Comment = entity.DecisionComment
    };
}
