namespace EvolutionService.Command.Application.Commands;

public record UpdateTrainingCommand(
    Guid Id,
    string Title,
    string Provider,
    string Status,
    DateTime StartDate,
    DateTime? EndDate,
    DateTime? CompletionDate,
    string? CertificateUrl,
    string? Comment);
