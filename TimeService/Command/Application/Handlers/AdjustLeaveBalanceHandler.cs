using Infrastructure.Api.Messaging;
using Infrastructure.Api.Persistence;
using Infrastructure.Api.Constants;
using TimeService.Command.Application.Abstractions;
using TimeService.Command.Application.Commands;
using TimeService.Command.Application.DTOs;
using TimeService.Command.Domain;
using TimeService.Command.Domain.Events;

namespace TimeService.Command.Application.Handlers;

public class AdjustLeaveBalanceHandler : ICommandHandler<AdjustLeaveBalanceCommand, CommandAcceptedResponse>
{
    private readonly ILeaveBalanceRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    public AdjustLeaveBalanceHandler(ILeaveBalanceRepository repository, IUnitOfWork unitOfWork)
        => (_repository, _unitOfWork) = (repository, unitOfWork);

    public async Task<CommandAcceptedResponse> HandleAsync(AdjustLeaveBalanceCommand command, CancellationToken cancellationToken = default)
    {
        var leaveType = LeaveTypeConstants.Normalize(command.LeaveType);
        var entity = await _repository.GetByEmployeeAndTypeAsync(command.EmployeeId, leaveType, cancellationToken);
        var isNew = entity is null;
        entity ??= LeaveBalance.Create(command.EmployeeId, leaveType);
        entity.Adjust(command.Delta);
        var evt = new LeaveBalanceAdjustedEvent
        {
            LeaveBalanceId = entity.Id,
            AccountId = command.EmployeeId,
            EmployeeId = entity.EmployeeId,
            LeaveType = entity.LeaveType,
            Available = entity.Available,
            Used = entity.Used,
            Pending = entity.Pending,
            Delta = command.Delta,
            Reason = command.Reason,
            UpdatedAt = entity.UpdatedAt
        };
        if (isNew)
            await _repository.AddAsync(entity, evt, cancellationToken);
        else
            await _repository.UpdateAsync(entity, evt, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new CommandAcceptedResponse(entity.Id, "Adjusted", "Leave balance adjusted.", evt);
    }
}
