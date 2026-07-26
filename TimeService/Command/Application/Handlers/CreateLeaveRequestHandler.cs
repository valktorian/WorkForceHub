using Infrastructure.Api.Authentication;
using Infrastructure.Api.Messaging;
using Infrastructure.Api.Persistence;
using TimeService.Command.Application.Abstractions;
using TimeService.Command.Application.Commands;
using TimeService.Command.Application.DTOs;
using TimeService.Command.Domain;

namespace TimeService.Command.Application.Handlers;

public class CreateLeaveRequestHandler : ICommandHandler<CreateLeaveRequestCommand, CommandAcceptedResponse>
{
    private readonly ITimeCommandRepository<LeaveRequest> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public CreateLeaveRequestHandler(ITimeCommandRepository<LeaveRequest> repository, IUnitOfWork unitOfWork, ICurrentUserAccessor currentUserAccessor)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<CommandAcceptedResponse> HandleAsync(CreateLeaveRequestCommand command, CancellationToken cancellationToken = default)
    {
        var entity = LeaveRequest.Create(_currentUserAccessor.GetRequiredAccountId(), command.EmployeeId,
            command.LeaveType, command.StartDate, command.EndDate, command.Reason);
        var evt = TimeEventFactory.Created(entity);
        await _repository.AddAsync(entity, evt, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new CommandAcceptedResponse(entity.Id, entity.Status, "Leave request created.", evt);
    }
}
