using EvolutionService.Command.Application.Abstractions;
using EvolutionService.Command.Application.Commands;
using EvolutionService.Command.Application.DTOs;
using EvolutionService.Command.Domain;
using EvolutionService.Command.Domain.Events;
using Infrastructure.Api.Common;
using Infrastructure.Api.Messaging;
using Infrastructure.Api.Persistence;

namespace EvolutionService.Command.Application.Handlers;

public class CreateTrainingHandler : ICommandHandler<CreateTrainingCommand, CommandAcceptedResponse>
{
    private readonly IEvolutionCommandRepository<Training> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTrainingHandler(IEvolutionCommandRepository<Training> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CommandAcceptedResponse> HandleAsync(CreateTrainingCommand command, CancellationToken cancellationToken = default)
    {
        var entity = Training.Create(command.EmployeeId, command.Title, command.Provider, command.Status, command.StartDate, command.EndDate, command.CompletionDate, command.CertificateUrl, command.Comment);
        var evt = ToEvent(entity);
        await _repository.AddAsync(entity, evt, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new CommandAcceptedResponse(entity.Id, "accepted", "Training created.", evt);
    }

    internal static TrainingUpsertedEvent ToEvent(Training entity) => new()
    {
        TrainingId = entity.Id,
        EmployeeId = entity.EmployeeId,
        Title = entity.Title,
        Provider = entity.Provider,
        Status = entity.Status,
        StartDate = entity.StartDate,
        EndDate = entity.EndDate,
        CompletionDate = entity.CompletionDate,
        CertificateUrl = entity.CertificateUrl,
        Comment = entity.Comment,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}

public class UpdateTrainingHandler : ICommandHandler<UpdateTrainingCommand, CommandAcceptedResponse>
{
    private readonly IEvolutionCommandRepository<Training> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTrainingHandler(IEvolutionCommandRepository<Training> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CommandAcceptedResponse> HandleAsync(UpdateTrainingCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw ApiException.NotFound("Training not found.");

        entity.Update(command.Title, command.Provider, command.Status, command.StartDate, command.EndDate, command.CompletionDate, command.CertificateUrl, command.Comment);
        var evt = CreateTrainingHandler.ToEvent(entity);
        await _repository.UpdateAsync(entity, evt, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new CommandAcceptedResponse(entity.Id, "accepted", "Training updated.", evt);
    }
}

public class DeleteTrainingHandler : ICommandHandler<DeleteTrainingCommand, bool>
{
    private readonly IEvolutionCommandRepository<Training> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTrainingHandler(IEvolutionCommandRepository<Training> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> HandleAsync(DeleteTrainingCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw ApiException.NotFound("Training not found.");

        var deletedAt = DateTime.UtcNow;
        await _repository.DeleteAsync(entity, new TrainingDeletedEvent { TrainingId = entity.Id, DeletedAt = deletedAt }, new { TrainingId = entity.Id, DeletedAt = deletedAt }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
