namespace TimeService.Command.Application.Commands;

public record DeleteTimeEntryCommand(
    Guid Id,
    Guid RequestedByAccountId,
    bool CanDeleteAny);
