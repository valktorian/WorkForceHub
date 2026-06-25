namespace EvolutionService.Command.Application.Commands;

public record UpdateRewardCommand(
    Guid Id,
    string Title,
    string Type,
    decimal Value,
    DateTime GrantedAt,
    string Reason,
    string? Comment);
