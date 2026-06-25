namespace EvolutionService.Command.Application.Commands;

public record UpdateJobMovementCommand(
    Guid Id,
    string PreviousJobTitle,
    string NewJobTitle,
    string PreviousDepartment,
    string NewDepartment,
    DateTime EffectiveDate,
    string Reason,
    string? Comment);
