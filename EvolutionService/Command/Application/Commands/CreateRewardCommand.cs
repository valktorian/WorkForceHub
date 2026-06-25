namespace EvolutionService.Command.Application.Commands;

public record CreateRewardCommand(
    Guid EmployeeId,
    string Title,
    string Type,
    decimal Value,
    DateTime GrantedAt,
    string Reason,
    string? Comment);
