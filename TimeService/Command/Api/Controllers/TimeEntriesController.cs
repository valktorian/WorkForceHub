using Infrastructure.Api.Authentication;
using Infrastructure.Api.Constants;
using Infrastructure.Api.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TimeService.Command.Application.Commands;
using TimeService.Command.Application.DTOs;

namespace TimeService.Command.Api.Controllers;

[ApiController]
[Route("api/time-entries")]
[Authorize]
public class TimeEntriesController : ControllerBase
{
    private readonly ICommandDispatcher _dispatcher;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public TimeEntriesController(
        ICommandDispatcher dispatcher,
        ICurrentUserAccessor currentUserAccessor)
    {
        _dispatcher = dispatcher;
        _currentUserAccessor = currentUserAccessor;
    }

    [HttpPost]
    [Authorize(Roles = RoleConstants.EmployeeManagerOrHrAdmin)]
    [SwaggerOperation(Summary = "Create a time entry.")]
    public async Task<IActionResult> Create([FromBody] CreateTimeEntryCommand command, CancellationToken ct)
        => Ok(await _dispatcher.SendAsync<CreateTimeEntryCommand, CommandAcceptedResponse>(command, ct));

    [HttpPut("{id:guid}")]
    [Authorize(Roles = RoleConstants.EmployeeManagerOrHrAdmin)]
    [SwaggerOperation(Summary = "Update a time entry.")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTimeEntryCommand command, CancellationToken ct)
        => Ok(await _dispatcher.SendAsync<UpdateTimeEntryCommand, CommandAcceptedResponse>(command with { Id = id }, ct));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = RoleConstants.EmployeeOrHrAdmin)]
    [SwaggerOperation(Summary = "Delete a time entry.")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var command = new DeleteTimeEntryCommand(
            id,
            _currentUserAccessor.GetRequiredAccountId(),
            User.IsInRole(RoleConstants.HrAdmin));

        await _dispatcher.SendAsync<DeleteTimeEntryCommand, CommandAcceptedResponse>(command, ct);
        return NoContent();
    }
}
