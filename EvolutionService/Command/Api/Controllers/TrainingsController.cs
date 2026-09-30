using EvolutionService.Command.Application.Commands;
using EvolutionService.Command.Application.DTOs;
using Infrastructure.Api.Constants;
using Infrastructure.Api.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EvolutionService.Command.Api.Controllers;

[ApiController]
[Route("api/trainings")]
[Authorize]
public class TrainingsController : ControllerBase
{
    private readonly ICommandDispatcher _dispatcher;

    public TrainingsController(ICommandDispatcher dispatcher) => _dispatcher = dispatcher;

    [HttpPost]
    [Authorize(Roles = RoleConstants.EvolutionUsers)]
    [SwaggerOperation(Summary = "Create a training record.")]
    public Task<IActionResult> Create([FromBody] CreateTrainingCommand command, CancellationToken ct)
        => Dispatch(command, ct);

    [HttpPut("{id:guid}")]
    [Authorize(Roles = RoleConstants.EvolutionUsers)]
    [SwaggerOperation(Summary = "Update a training record.")]
    public Task<IActionResult> Update(Guid id, [FromBody] UpdateTrainingCommand command, CancellationToken ct)
        => Dispatch(command with { Id = id }, ct);

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = RoleConstants.EvolutionUsers)]
    [SwaggerOperation(Summary = "Delete a training record.")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _dispatcher.SendAsync<DeleteTrainingCommand, bool>(new DeleteTrainingCommand(id), ct);
        return NoContent();
    }

    private async Task<IActionResult> Dispatch<TCommand>(TCommand command, CancellationToken ct)
        => Ok(await _dispatcher.SendAsync<TCommand, CommandAcceptedResponse>(command, ct));
}
