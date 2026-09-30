using EvolutionService.Command.Application.Commands;
using EvolutionService.Command.Application.DTOs;
using Infrastructure.Api.Constants;
using Infrastructure.Api.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EvolutionService.Command.Api.Controllers;

[ApiController]
[Route("api/salary-changes")]
[Authorize]
public class SalaryChangesController : ControllerBase
{
    private readonly ICommandDispatcher _dispatcher;

    public SalaryChangesController(ICommandDispatcher dispatcher) => _dispatcher = dispatcher;

    [HttpPost]
    [Authorize(Roles = RoleConstants.EvolutionUsers)]
    [SwaggerOperation(Summary = "Create a salary change.")]
    public Task<IActionResult> Create([FromBody] CreateSalaryChangeCommand command, CancellationToken ct)
        => Dispatch(command, ct);

    [HttpPut("{id:guid}")]
    [Authorize(Roles = RoleConstants.EvolutionUsers)]
    [SwaggerOperation(Summary = "Update a salary change.")]
    public Task<IActionResult> Update(Guid id, [FromBody] UpdateSalaryChangeCommand command, CancellationToken ct)
        => Dispatch(command with { Id = id }, ct);

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = RoleConstants.EvolutionUsers)]
    [SwaggerOperation(Summary = "Delete a salary change.")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _dispatcher.SendAsync<DeleteSalaryChangeCommand, bool>(new DeleteSalaryChangeCommand(id), ct);
        return NoContent();
    }

    private async Task<IActionResult> Dispatch<TCommand>(TCommand command, CancellationToken ct)
        => Ok(await _dispatcher.SendAsync<TCommand, CommandAcceptedResponse>(command, ct));
}
