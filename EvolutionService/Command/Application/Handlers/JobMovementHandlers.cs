using EvolutionService.Command.Application.Abstractions;
using EvolutionService.Command.Application.Commands;
using EvolutionService.Command.Application.DTOs;
using EvolutionService.Command.Domain;
using EvolutionService.Command.Domain.Events;
using Infrastructure.Api.Common;
using Infrastructure.Api.Messaging;
using Infrastructure.Api.Persistence;

namespace EvolutionService.Command.Application.Handlers;

public class CreateJobMovementHandler : ICommandHandler<CreateJobMovementCommand, CommandAcceptedResponse>
{
    private readonly IEvolutionCommandRepository<JobMovement> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateJobMovementHandler(IEvolutionCommandRepository<JobMovement> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CommandAcceptedResponse> HandleAsync(CreateJobMovementCommand command, CancellationToken cancellationToken = default)
    {
        var entity = JobMovement.Create(command.EmployeeId, command.PreviousJobTitle, command.NewJobTitle, command.PreviousDepartment, command.NewDepartment, command.EffectiveDate, command.Reason, command.Comment);
        var evt = ToEvent(entity);
        await _repository.AddAsync(entity, evt, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new CommandAcceptedResponse(entity.Id, "accepted", "Job movement created.", evt);
    }

    internal static JobMovementUpsertedEvent ToEvent(JobMovement entity) => new()
    {
        JobMovementId = entity.Id,
        EmployeeId = entity.EmployeeId,
        PreviousJobTitle = entity.PreviousJobTitle,
        NewJobTitle = entity.NewJobTitle,
        PreviousDepartment = entity.PreviousDepartment,
        NewDepartment = entity.NewDepartment,
        EffectiveDate = entity.EffectiveDate,
        Reason = entity.Reason,
        Comment = entity.Comment,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}

public class UpdateJobMovementHandler : ICommandHandler<UpdateJobMovementCommand, CommandAcceptedResponse>
{
    private readonly IEvolutionCommandRepository<JobMovement> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateJobMovementHandler(IEvolutionCommandRepository<JobMovement> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CommandAcceptedResponse> HandleAsync(UpdateJobMovementCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw ApiException.NotFound("Job movement not found.");

        entity.Update(command.PreviousJobTitle, command.NewJobTitle, command.PreviousDepartment, command.NewDepartment, command.EffectiveDate, command.Reason, command.Comment);
        var evt = CreateJobMovementHandler.ToEvent(entity);
        await _repository.UpdateAsync(entity, evt, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new CommandAcceptedResponse(entity.Id, "accepted", "Job movement updated.", evt);
    }
}

public class DeleteJobMovementHandler : ICommandHandler<DeleteJobMovementCommand, bool>
{
    private readonly IEvolutionCommandRepository<JobMovement> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteJobMovementHandler(IEvolutionCommandRepository<JobMovement> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> HandleAsync(DeleteJobMovementCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw ApiException.NotFound("Job movement not found.");

        var deletedAt = DateTime.UtcNow;
        await _repository.DeleteAsync(entity, new JobMovementDeletedEvent { JobMovementId = entity.Id, DeletedAt = deletedAt }, new { JobMovementId = entity.Id, DeletedAt = deletedAt }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
