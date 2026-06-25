using EvolutionService.Command.Application.Commands;
using EvolutionService.Command.Application.DTOs;
using Infrastructure.Api.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EvolutionService.Command.Api.Controllers;

[ApiController]
[Route("api/salary-changes")]
[Authorize]
public class SalaryChangesController : ControllerBase
{
    private const string WriterRoles = "HRAdmin,HRManager,Manager";
    private readonly ICommandDispatcher _dispatcher;

    public SalaryChangesController(ICommandDispatcher dispatcher) => _dispatcher = dispatcher;

    [HttpPost]
    [Authorize(Roles = WriterRoles)]
    public Task<IActionResult> Create([FromBody] CreateSalaryChangeCommand command, CancellationToken ct)
        => Dispatch(command, ct);

    [HttpPut("{id:guid}")]
    [Authorize(Roles = WriterRoles)]
    public Task<IActionResult> Update(Guid id, [FromBody] UpdateSalaryChangeCommand command, CancellationToken ct)
        => Dispatch(command with { Id = id }, ct);

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = WriterRoles)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _dispatcher.SendAsync<DeleteSalaryChangeCommand, bool>(new DeleteSalaryChangeCommand(id), ct);
        return NoContent();
    }

    private async Task<IActionResult> Dispatch<TCommand>(TCommand command, CancellationToken ct)
        => Ok(await _dispatcher.SendAsync<TCommand, CommandAcceptedResponse>(command, ct));
}
