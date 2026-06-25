using EvolutionService.Command.Application.Abstractions;
using EvolutionService.Command.Application.Commands;
using EvolutionService.Command.Application.DTOs;
using EvolutionService.Command.Domain;
using EvolutionService.Command.Domain.Events;
using Infrastructure.Api.Common;
using Infrastructure.Api.Messaging;
using Infrastructure.Api.Persistence;

namespace EvolutionService.Command.Application.Handlers;

public class CreateRewardHandler : ICommandHandler<CreateRewardCommand, CommandAcceptedResponse>
{
    private readonly IEvolutionCommandRepository<Reward> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateRewardHandler(IEvolutionCommandRepository<Reward> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CommandAcceptedResponse> HandleAsync(CreateRewardCommand command, CancellationToken cancellationToken = default)
    {
        var entity = Reward.Create(command.EmployeeId, command.Title, command.Type, command.Value, command.GrantedAt, command.Reason, command.Comment);
        var evt = ToEvent(entity);
        await _repository.AddAsync(entity, evt, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new CommandAcceptedResponse(entity.Id, "accepted", "Reward created.", evt);
    }

    internal static RewardUpsertedEvent ToEvent(Reward entity) => new()
    {
        RewardId = entity.Id,
        EmployeeId = entity.EmployeeId,
        Title = entity.Title,
        Type = entity.Type,
        Value = entity.Value,
        GrantedAt = entity.GrantedAt,
        Reason = entity.Reason,
        Comment = entity.Comment,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}

public class UpdateRewardHandler : ICommandHandler<UpdateRewardCommand, CommandAcceptedResponse>
{
    private readonly IEvolutionCommandRepository<Reward> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateRewardHandler(IEvolutionCommandRepository<Reward> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CommandAcceptedResponse> HandleAsync(UpdateRewardCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw ApiException.NotFound("Reward not found.");

        entity.Update(command.Title, command.Type, command.Value, command.GrantedAt, command.Reason, command.Comment);
        var evt = CreateRewardHandler.ToEvent(entity);
        await _repository.UpdateAsync(entity, evt, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new CommandAcceptedResponse(entity.Id, "accepted", "Reward updated.", evt);
    }
}

public class DeleteRewardHandler : ICommandHandler<DeleteRewardCommand, bool>
{
    private readonly IEvolutionCommandRepository<Reward> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteRewardHandler(IEvolutionCommandRepository<Reward> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> HandleAsync(DeleteRewardCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw ApiException.NotFound("Reward not found.");

        var deletedAt = DateTime.UtcNow;
        await _repository.DeleteAsync(entity, new RewardDeletedEvent { RewardId = entity.Id, DeletedAt = deletedAt }, new { RewardId = entity.Id, DeletedAt = deletedAt }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
