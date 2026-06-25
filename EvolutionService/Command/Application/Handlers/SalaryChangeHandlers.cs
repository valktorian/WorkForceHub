using EvolutionService.Command.Application.Abstractions;
using EvolutionService.Command.Application.Commands;
using EvolutionService.Command.Application.DTOs;
using EvolutionService.Command.Domain;
using EvolutionService.Command.Domain.Events;
using Infrastructure.Api.Common;
using Infrastructure.Api.Messaging;
using Infrastructure.Api.Persistence;

namespace EvolutionService.Command.Application.Handlers;

public class CreateSalaryChangeHandler : ICommandHandler<CreateSalaryChangeCommand, CommandAcceptedResponse>
{
    private readonly IEvolutionCommandRepository<SalaryChange> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSalaryChangeHandler(IEvolutionCommandRepository<SalaryChange> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CommandAcceptedResponse> HandleAsync(CreateSalaryChangeCommand command, CancellationToken cancellationToken = default)
    {
        var entity = SalaryChange.Create(command.EmployeeId, command.PreviousSalary, command.NewSalary, command.Currency, command.EffectiveDate, command.Reason, command.Comment);
        var evt = ToEvent(entity);
        await _repository.AddAsync(entity, evt, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new CommandAcceptedResponse(entity.Id, "accepted", "Salary change created.", evt);
    }

    internal static SalaryChangeUpsertedEvent ToEvent(SalaryChange entity) => new()
    {
        SalaryChangeId = entity.Id,
        EmployeeId = entity.EmployeeId,
        PreviousSalary = entity.PreviousSalary,
        NewSalary = entity.NewSalary,
        Currency = entity.Currency,
        EffectiveDate = entity.EffectiveDate,
        Reason = entity.Reason,
        Comment = entity.Comment,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}

public class UpdateSalaryChangeHandler : ICommandHandler<UpdateSalaryChangeCommand, CommandAcceptedResponse>
{
    private readonly IEvolutionCommandRepository<SalaryChange> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSalaryChangeHandler(IEvolutionCommandRepository<SalaryChange> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CommandAcceptedResponse> HandleAsync(UpdateSalaryChangeCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw ApiException.NotFound("Salary change not found.");

        entity.Update(command.PreviousSalary, command.NewSalary, command.Currency, command.EffectiveDate, command.Reason, command.Comment);
        var evt = CreateSalaryChangeHandler.ToEvent(entity);
        await _repository.UpdateAsync(entity, evt, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new CommandAcceptedResponse(entity.Id, "accepted", "Salary change updated.", evt);
    }
}

public class DeleteSalaryChangeHandler : ICommandHandler<DeleteSalaryChangeCommand, bool>
{
    private readonly IEvolutionCommandRepository<SalaryChange> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteSalaryChangeHandler(IEvolutionCommandRepository<SalaryChange> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> HandleAsync(DeleteSalaryChangeCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw ApiException.NotFound("Salary change not found.");

        var deletedAt = DateTime.UtcNow;
        await _repository.DeleteAsync(entity, new SalaryChangeDeletedEvent { SalaryChangeId = entity.Id, DeletedAt = deletedAt }, new { SalaryChangeId = entity.Id, DeletedAt = deletedAt }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
