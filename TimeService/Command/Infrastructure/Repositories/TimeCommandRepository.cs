using Infrastructure.Api.Base;
using Infrastructure.Api.Messaging;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TimeService.Command.Application.Abstractions;
using TimeService.Command.Domain;
using TimeService.Command.Infrastructure.Persistence;

namespace TimeService.Command.Infrastructure.Repositories;

public class TimeCommandRepository<TEntity> : ITimeCommandRepository<TEntity>
    where TEntity : BaseEntity
{
    protected readonly TimeCommandDbContext DbContext;

    public TimeCommandRepository(TimeCommandDbContext dbContext)
    {
        DbContext = dbContext;
    }

    public async Task AddAsync(TEntity entity, BaseEvent evt, CancellationToken cancellationToken = default)
    {
        await DbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
        await AddOutboxAsync(entity.Id, evt, evt, cancellationToken);
    }

    public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => DbContext.Set<TEntity>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task UpdateAsync(TEntity entity, BaseEvent evt, CancellationToken cancellationToken = default)
        => AddOutboxAsync(entity.Id, evt, evt, cancellationToken);

    public async Task DeleteAsync(
        TEntity entity,
        BaseEvent evt,
        object payload,
        CancellationToken cancellationToken = default)
    {
        DbContext.Set<TEntity>().Remove(entity);
        await AddOutboxAsync(entity.Id, evt, payload, cancellationToken);
    }

    protected Task AddOutboxAsync(
        Guid aggregateId,
        BaseEvent evt,
        object payload,
        CancellationToken cancellationToken)
    {
        return DbContext.OutboxMessages.AddAsync(new OutboxMessage
        {
            AggregateType = typeof(TEntity).Name,
            AggregateId = aggregateId,
            EventType = evt.GetType().AssemblyQualifiedName!,
            Payload = JsonSerializer.Serialize(payload),
            OccurredAt = evt.OccurredAt
        }, cancellationToken).AsTask();
    }
}

public sealed class LeaveBalanceRepository
    : TimeCommandRepository<LeaveBalance>, ILeaveBalanceRepository
{
    public LeaveBalanceRepository(TimeCommandDbContext dbContext)
        : base(dbContext)
    {
    }

    public Task<LeaveBalance?> GetByEmployeeAndTypeAsync(
        Guid employeeId,
        string leaveType,
        CancellationToken cancellationToken = default)
    {
        var normalizedLeaveType = leaveType.Trim();
        return DbContext.LeaveBalances.FirstOrDefaultAsync(
            x => x.EmployeeId == employeeId && x.LeaveType == normalizedLeaveType,
            cancellationToken);
    }
}
