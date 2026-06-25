namespace EvolutionService.Command.Application.Commands;

public record UpdateSalaryChangeCommand(
    Guid Id,
    decimal PreviousSalary,
    decimal NewSalary,
    string Currency,
    DateTime EffectiveDate,
    string Reason,
    string? Comment);
