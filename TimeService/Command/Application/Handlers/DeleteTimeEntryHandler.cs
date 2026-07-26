using Infrastructure.Api.Common;
using Infrastructure.Api.Messaging;
using Infrastructure.Api.Persistence;
using TimeService.Command.Application.Abstractions;
using TimeService.Command.Application.Commands;
using TimeService.Command.Application.DTOs;
using TimeService.Command.Domain;
using TimeService.Command.Domain.Events;

namespace TimeService.Command.Application.Handlers;

public class DeleteTimeEntryHandler : ICommandHandler<DeleteTimeEntryCommand, CommandAcceptedResponse>
{
    private readonly ITimeCommandRepository<TimeEntry> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTimeEntryHandler(ITimeCommandRepository<TimeEntry> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CommandAcceptedResponse> HandleAsync(
        DeleteTimeEntryCommand command,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw ApiException.NotFound("Time entry not found.");
        if (!command.CanDeleteAny && entity.AccountId != command.RequestedByAccountId)
        {
            throw new ApiException("You can only delete your own time entries.", 403);
        }

        var evt = new TimeEntryDeletedEvent { TimeEntryId = entity.Id, DeletedAt = DateTime.UtcNow };
        await _repository.DeleteAsync(entity, evt, evt, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new CommandAcceptedResponse(entity.Id, "Deleted", "Time entry deleted.", evt);
    }
}
