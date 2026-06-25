namespace EvolutionService.Command.Application.Commands;

public record CreateSalaryChangeCommand(
    Guid EmployeeId,
    decimal PreviousSalary,
    decimal NewSalary,
    string Currency,
    DateTime EffectiveDate,
    string Reason,
    string? Comment);
