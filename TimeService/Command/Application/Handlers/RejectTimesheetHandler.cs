using Infrastructure.Api.Common;
using Infrastructure.Api.Messaging;
using Infrastructure.Api.Persistence;
using TimeService.Command.Application.Abstractions;
using TimeService.Command.Application.Commands;
using TimeService.Command.Application.DTOs;
using TimeService.Command.Domain;

namespace TimeService.Command.Application.Handlers;

public class RejectTimesheetHandler : ICommandHandler<RejectTimesheetCommand, CommandAcceptedResponse>
{
    private readonly ITimeCommandRepository<Timesheet> _repository;
    private readonly IUnitOfWork _unitOfWork;
    public RejectTimesheetHandler(ITimeCommandRepository<Timesheet> repository, IUnitOfWork unitOfWork)
        => (_repository, _unitOfWork) = (repository, unitOfWork);

    public async Task<CommandAcceptedResponse> HandleAsync(RejectTimesheetCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw ApiException.NotFound("Timesheet not found.");
        entity.Reject(command.Comment);
        var evt = TimeEventFactory.Status(entity);
        await _repository.UpdateAsync(entity, evt, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new CommandAcceptedResponse(entity.Id, entity.Status, "Timesheet rejected.", evt);
    }
}
