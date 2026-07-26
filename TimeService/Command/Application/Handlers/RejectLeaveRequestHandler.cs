using Infrastructure.Api.Common;
using Infrastructure.Api.Messaging;
using Infrastructure.Api.Persistence;
using TimeService.Command.Application.Abstractions;
using TimeService.Command.Application.Commands;
using TimeService.Command.Application.DTOs;
using TimeService.Command.Domain;

namespace TimeService.Command.Application.Handlers;

public class RejectLeaveRequestHandler : ICommandHandler<RejectLeaveRequestCommand, CommandAcceptedResponse>
{
    private readonly ITimeCommandRepository<LeaveRequest> _repository;
    private readonly IUnitOfWork _unitOfWork;
    public RejectLeaveRequestHandler(ITimeCommandRepository<LeaveRequest> repository, IUnitOfWork unitOfWork)
        => (_repository, _unitOfWork) = (repository, unitOfWork);

    public async Task<CommandAcceptedResponse> HandleAsync(RejectLeaveRequestCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw ApiException.NotFound("Leave request not found.");
        entity.Reject(command.Comment);
        var evt = TimeEventFactory.Status(entity);
        await _repository.UpdateAsync(entity, evt, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new CommandAcceptedResponse(entity.Id, entity.Status, "Leave request rejected.", evt);
    }
}
