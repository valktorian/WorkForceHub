namespace EvolutionService.Command.Application.Commands;

public record CreateJobMovementCommand(
    Guid EmployeeId,
    string PreviousJobTitle,
    string NewJobTitle,
    string PreviousDepartment,
    string NewDepartment,
    DateTime EffectiveDate,
    string Reason,
    string? Comment);
