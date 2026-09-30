using EvolutionService.Command.Application.Commands;
using EvolutionService.Command.Application.DTOs;
using Infrastructure.Api.Constants;
using Infrastructure.Api.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EvolutionService.Command.Api.Controllers;

[ApiController]
[Route("api/job-movements")]
[Authorize]
public class JobMovementsController : ControllerBase
{
    private readonly ICommandDispatcher _dispatcher;

    public JobMovementsController(ICommandDispatcher dispatcher) => _dispatcher = dispatcher;

    [HttpPost]
    [Authorize(Roles = RoleConstants.EvolutionUsers)]
    [SwaggerOperation(Summary = "Create a job movement.")]
    public Task<IActionResult> Create([FromBody] CreateJobMovementCommand command, CancellationToken ct)
        => Dispatch(command, ct);

    [HttpPut("{id:guid}")]
    [Authorize(Roles = RoleConstants.EvolutionUsers)]
    [SwaggerOperation(Summary = "Update a job movement.")]
    public Task<IActionResult> Update(Guid id, [FromBody] UpdateJobMovementCommand command, CancellationToken ct)
        => Dispatch(command with { Id = id }, ct);

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = RoleConstants.EvolutionUsers)]
    [SwaggerOperation(Summary = "Delete a job movement.")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _dispatcher.SendAsync<DeleteJobMovementCommand, bool>(new DeleteJobMovementCommand(id), ct);
        return NoContent();
    }

    private async Task<IActionResult> Dispatch<TCommand>(TCommand command, CancellationToken ct)
        => Ok(await _dispatcher.SendAsync<TCommand, CommandAcceptedResponse>(command, ct));
}
