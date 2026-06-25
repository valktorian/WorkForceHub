namespace EvolutionService.Command.Application.Commands;

public record CreateTrainingCommand(
    Guid EmployeeId,
    string Title,
    string Provider,
    string Status,
    DateTime StartDate,
    DateTime? EndDate,
    DateTime? CompletionDate,
    string? CertificateUrl,
    string? Comment);
