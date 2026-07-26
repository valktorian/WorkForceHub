using Infrastructure.Api.Constants;
using Infrastructure.Api.Mapping;
using Infrastructure.Api.Messaging;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.Text.Json;
using TimeService.Query.Domain;
using TimeService.Query.Infrastructure;

namespace TimeService.Query.Application.Consumers;

public class TimeEventConsumer : IEventHandler
{
    private readonly ReadDbContext _readDb;
    private readonly ILogger<TimeEventConsumer> _logger;

    public string EventType => EventTypeConstants.Time.TimeEntryCreated;

    public TimeEventConsumer(ReadDbContext readDb, ILogger<TimeEventConsumer> logger)
    {
        _readDb = readDb;
        _logger = logger;
    }

    public async Task HandleAsync(JsonElement payload)
    {
        try
        {
            if (payload.TryGetProperty("TimeEntryId", out _))
            {
                if (payload.TryGetProperty("DeletedAt", out _))
                {
                    await DeleteTimeEntryAsync(payload);
                    return;
                }

                await UpsertTimeEntryAsync(payload);
                return;
            }

            if (payload.TryGetProperty("TimesheetId", out _))
            {
                if (payload.TryGetProperty("PeriodStart", out _))
                {
                    await UpsertTimesheetAsync(payload);
                }
                else
                {
                    await UpdateTimesheetStatusAsync(payload);
                }
                return;
            }

            if (payload.TryGetProperty("LeaveRequestId", out _))
            {
                if (payload.TryGetProperty("StartDate", out _))
                {
                    await UpsertLeaveRequestAsync(payload);
                }
                else
                {
                    await UpdateLeaveRequestStatusAsync(payload);
                }
                return;
            }

            if (payload.TryGetProperty("LeaveBalanceId", out _))
            {
                await UpsertLeaveBalanceAsync(payload);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling time event payload.");
        }
    }

    private async Task UpsertTimeEntryAsync(JsonElement payload)
    {
        var model = JsonElementMapper.Map<TimeEntryReadModel>(payload, static (source, readModel) =>
        {
            readModel.Id = source.GetRequiredGuid("TimeEntryId");
        });

        await _readDb.TimeEntries.ReplaceOneAsync(x => x.Id == model.Id, model, new ReplaceOptions { IsUpsert = true });
    }

    private async Task DeleteTimeEntryAsync(JsonElement payload)
    {
        var timeEntryId = payload.GetProperty("TimeEntryId").GetGuid();
        var result = await _readDb.TimeEntries.DeleteOneAsync(x => x.Id == timeEntryId);
        if (result.DeletedCount == 0)
        {
            _logger.LogWarning("Time entry projection {TimeEntryId} was already absent.", timeEntryId);
        }
    }

    private async Task UpsertTimesheetAsync(JsonElement payload)
    {
        var model = JsonElementMapper.Map<TimesheetReadModel>(payload, static (source, readModel) =>
        {
            readModel.Id = source.GetRequiredGuid("TimesheetId");
        });

        await _readDb.Timesheets.ReplaceOneAsync(x => x.Id == model.Id, model, new ReplaceOptions { IsUpsert = true });
    }

    private async Task UpsertLeaveRequestAsync(JsonElement payload)
    {
        var model = JsonElementMapper.Map<LeaveRequestReadModel>(payload, static (source, readModel) =>
        {
            readModel.Id = source.GetRequiredGuid("LeaveRequestId");
        });

        await _readDb.LeaveRequests.ReplaceOneAsync(x => x.Id == model.Id, model, new ReplaceOptions { IsUpsert = true });
    }

    private async Task UpdateTimesheetStatusAsync(JsonElement payload)
    {
        var timesheetId = payload.GetProperty("TimesheetId").GetGuid();
        var update = Builders<TimesheetReadModel>.Update
            .Set(x => x.Status, payload.GetProperty("Status").GetString() ?? string.Empty)
            .Set(x => x.UpdatedAt, payload.GetProperty("UpdatedAt").GetDateTime());

        if (payload.TryGetProperty("SubmittedAt", out var submittedAt) && submittedAt.ValueKind != JsonValueKind.Null)
        {
            update = update.Set(x => x.SubmittedAt, submittedAt.GetDateTime());
        }

        if (payload.TryGetProperty("ApprovedAt", out var approvedAt) && approvedAt.ValueKind != JsonValueKind.Null)
        {
            update = update.Set(x => x.ApprovedAt, approvedAt.GetDateTime());
        }

        await _readDb.Timesheets.UpdateOneAsync(x => x.Id == timesheetId, update);
    }

    private async Task UpdateLeaveRequestStatusAsync(JsonElement payload)
    {
        var leaveRequestId = payload.GetProperty("LeaveRequestId").GetGuid();
        var update = Builders<LeaveRequestReadModel>.Update
            .Set(x => x.Status, payload.GetProperty("Status").GetString() ?? string.Empty)
            .Set(x => x.UpdatedAt, payload.GetProperty("UpdatedAt").GetDateTime());

        if (payload.TryGetProperty("SubmittedAt", out var submittedAt) && submittedAt.ValueKind != JsonValueKind.Null)
        {
            update = update.Set(x => x.SubmittedAt, submittedAt.GetDateTime());
        }

        if (payload.TryGetProperty("DecisionAt", out var decisionAt) && decisionAt.ValueKind != JsonValueKind.Null)
        {
            update = update.Set(x => x.DecisionAt, decisionAt.GetDateTime());
        }

        await _readDb.LeaveRequests.UpdateOneAsync(x => x.Id == leaveRequestId, update);
    }

    private async Task UpsertLeaveBalanceAsync(JsonElement payload)
    {
        var leaveBalanceId = payload.GetProperty("LeaveBalanceId").GetGuid();
        var employeeId = payload.GetProperty("EmployeeId").GetGuid();
        var leaveType = payload.GetProperty("LeaveType").GetString() ?? string.Empty;
        var existing = await _readDb.LeaveBalances
            .Find(x => x.EmployeeId == employeeId && x.LeaveType == leaveType)
            .FirstOrDefaultAsync();

        var model = existing ?? JsonElementMapper.Map<LeaveBalanceReadModel>(payload, static (source, readModel) =>
        {
            readModel.Id = source.GetRequiredGuid("LeaveBalanceId");
        });

        model.AccountId ??= payload.GetOptionalGuid("AccountId");
        model.Available = payload.GetProperty("Available").GetDecimal();
        model.Used = payload.GetProperty("Used").GetDecimal();
        model.Pending = payload.GetProperty("Pending").GetDecimal();
        model.UpdatedAt = payload.GetProperty("UpdatedAt").GetDateTime();

        await _readDb.LeaveBalances.ReplaceOneAsync(
            x => x.EmployeeId == employeeId && x.LeaveType == leaveType,
            model,
            new ReplaceOptions { IsUpsert = true });
    }
}
