using Infrastructure.Api.Common;
using Infrastructure.Api.Messaging;
using Infrastructure.Api.Persistence;
using TimeService.Command.Application.Abstractions;
using TimeService.Command.Application.Commands;
using TimeService.Command.Application.DTOs;
using TimeService.Command.Domain;

namespace TimeService.Command.Application.Handlers;

public class UpdateTimeEntryHandler : ICommandHandler<UpdateTimeEntryCommand, CommandAcceptedResponse>
{
    private readonly ITimeCommandRepository<TimeEntry> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTimeEntryHandler(ITimeCommandRepository<TimeEntry> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CommandAcceptedResponse> HandleAsync(UpdateTimeEntryCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw ApiException.NotFound("Time entry not found.");
        entity.Update(command.WorkDate, command.StartTime, command.EndTime,
            command.ProjectCode, command.TaskCode, command.Notes);
        var evt = TimeEventFactory.Updated(entity);
        await _repository.UpdateAsync(entity, evt, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new CommandAcceptedResponse(entity.Id, entity.Status, "Time entry updated.", evt);
    }
}
