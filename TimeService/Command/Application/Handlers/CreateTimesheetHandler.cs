using Infrastructure.Api.Authentication;
using Infrastructure.Api.Messaging;
using Infrastructure.Api.Persistence;
using TimeService.Command.Application.Abstractions;
using TimeService.Command.Application.Commands;
using TimeService.Command.Application.DTOs;
using TimeService.Command.Domain;

namespace TimeService.Command.Application.Handlers;

public class CreateTimesheetHandler : ICommandHandler<CreateTimesheetCommand, CommandAcceptedResponse>
{
    private readonly ITimeCommandRepository<Timesheet> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public CreateTimesheetHandler(ITimeCommandRepository<Timesheet> repository, IUnitOfWork unitOfWork, ICurrentUserAccessor currentUserAccessor)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<CommandAcceptedResponse> HandleAsync(CreateTimesheetCommand command, CancellationToken cancellationToken = default)
    {
        var entity = Timesheet.Create(_currentUserAccessor.GetRequiredAccountId(), command.EmployeeId,
            command.PeriodStart, command.PeriodEnd);
        var evt = TimeEventFactory.Created(entity);
        await _repository.AddAsync(entity, evt, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new CommandAcceptedResponse(entity.Id, entity.Status, "Timesheet created.", evt);
    }
}
