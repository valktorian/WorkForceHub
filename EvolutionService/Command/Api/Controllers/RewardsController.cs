using EvolutionService.Command.Application.Commands;
using EvolutionService.Command.Application.DTOs;
using Infrastructure.Api.Constants;
using Infrastructure.Api.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EvolutionService.Command.Api.Controllers;

[ApiController]
[Route("api/rewards")]
[Authorize]
public class RewardsController : ControllerBase
{
    private readonly ICommandDispatcher _dispatcher;

    public RewardsController(ICommandDispatcher dispatcher) => _dispatcher = dispatcher;

    [HttpPost]
    [Authorize(Roles = RoleConstants.EvolutionUsers)]
    [SwaggerOperation(Summary = "Create a reward.")]
    public Task<IActionResult> Create([FromBody] CreateRewardCommand command, CancellationToken ct)
        => Dispatch(command, ct);

    [HttpPut("{id:guid}")]
    [Authorize(Roles = RoleConstants.EvolutionUsers)]
    [SwaggerOperation(Summary = "Update a reward.")]
    public Task<IActionResult> Update(Guid id, [FromBody] UpdateRewardCommand command, CancellationToken ct)
        => Dispatch(command with { Id = id }, ct);

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = RoleConstants.EvolutionUsers)]
    [SwaggerOperation(Summary = "Delete a reward.")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _dispatcher.SendAsync<DeleteRewardCommand, bool>(new DeleteRewardCommand(id), ct);
        return NoContent();
    }

    private async Task<IActionResult> Dispatch<TCommand>(TCommand command, CancellationToken ct)
        => Ok(await _dispatcher.SendAsync<TCommand, CommandAcceptedResponse>(command, ct));
}
