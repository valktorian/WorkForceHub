using Infrastructure.Api.Authentication;
using Infrastructure.Api.Messaging;
using Infrastructure.Api.Persistence;
using TimeService.Command.Application.Abstractions;
using TimeService.Command.Application.Commands;
using TimeService.Command.Application.DTOs;
using TimeService.Command.Domain;

namespace TimeService.Command.Application.Handlers;

public class CreateTimeEntryHandler : ICommandHandler<CreateTimeEntryCommand, CommandAcceptedResponse>
{
    private readonly ITimeCommandRepository<TimeEntry> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public CreateTimeEntryHandler(
        ITimeCommandRepository<TimeEntry> repository,
        IUnitOfWork unitOfWork,
        ICurrentUserAccessor currentUserAccessor)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<CommandAcceptedResponse> HandleAsync(CreateTimeEntryCommand command, CancellationToken cancellationToken = default)
    {
        var entity = TimeEntry.Create(_currentUserAccessor.GetRequiredAccountId(), command.EmployeeId,
            command.WorkDate, command.StartTime, command.EndTime, command.ProjectCode, command.TaskCode, command.Notes);
        var evt = TimeEventFactory.Created(entity);
        await _repository.AddAsync(entity, evt, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new CommandAcceptedResponse(entity.Id, entity.Status, "Time entry created.", evt);
    }
}
