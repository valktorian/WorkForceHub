namespace EvolutionService.Command.Application.DTOs;

public record CommandAcceptedResponse(Guid Id, string Status, string Message, object? Data = null);
