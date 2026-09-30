using Infrastructure.Api.Constants;
using Infrastructure.Api.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TimeService.Command.Api.Contracts;
using TimeService.Command.Application.Commands;
using TimeService.Command.Application.DTOs;

namespace TimeService.Command.Api.Controllers;

[ApiController]
[Route("api/timesheets")]
[Authorize]
public class TimesheetsController : ControllerBase
{
    private readonly ICommandDispatcher _dispatcher;

    public TimesheetsController(ICommandDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpPost]
    [Authorize(Roles = RoleConstants.TimeUsers)]
    [SwaggerOperation(Summary = "Create a timesheet.")]
    public async Task<IActionResult> Create([FromBody] CreateTimesheetCommand command, CancellationToken ct)
        => Ok(await _dispatcher.SendAsync<CreateTimesheetCommand, CommandAcceptedResponse>(command, ct));

    [HttpPost("{id:guid}/submit")]
    [Authorize(Roles = RoleConstants.TimeUsers)]
    [SwaggerOperation(Summary = "Submit a timesheet.")]
    public async Task<IActionResult> Submit(Guid id, CancellationToken ct)
        => Ok(await _dispatcher.SendAsync<SubmitTimesheetCommand, CommandAcceptedResponse>(new SubmitTimesheetCommand(id), ct));

    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = RoleConstants.TimeReviewers)]
    [SwaggerOperation(Summary = "Approve a timesheet.")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ReviewDecisionRequest request, CancellationToken ct)
        => Ok(await _dispatcher.SendAsync<ApproveTimesheetCommand, CommandAcceptedResponse>(new ApproveTimesheetCommand(id, request.Comment), ct));

    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = RoleConstants.TimeReviewers)]
    [SwaggerOperation(Summary = "Reject a timesheet.")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] ReviewDecisionRequest request, CancellationToken ct)
        => Ok(await _dispatcher.SendAsync<RejectTimesheetCommand, CommandAcceptedResponse>(new RejectTimesheetCommand(id, request.Comment), ct));

    [HttpPost("{id:guid}/reopen")]
    [Authorize(Roles = RoleConstants.TimeAdmins)]
    [SwaggerOperation(Summary = "Reopen a timesheet.")]
    public async Task<IActionResult> Reopen(Guid id, [FromBody] ReviewDecisionRequest request, CancellationToken ct)
        => Ok(await _dispatcher.SendAsync<ReopenTimesheetCommand, CommandAcceptedResponse>(new ReopenTimesheetCommand(id, request.Comment), ct));
}
