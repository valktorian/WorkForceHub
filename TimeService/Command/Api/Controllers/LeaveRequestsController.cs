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
[Route("api/leave-requests")]
[Authorize]
public class LeaveRequestsController : ControllerBase
{
    private readonly ICommandDispatcher _dispatcher;

    public LeaveRequestsController(ICommandDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpPost]
    [Authorize(Roles = RoleConstants.TimeUsers)]
    [SwaggerOperation(Summary = "Create a leave request.")]
    public async Task<IActionResult> Create([FromBody] CreateLeaveRequestCommand command, CancellationToken ct)
        => Ok(await _dispatcher.SendAsync<CreateLeaveRequestCommand, CommandAcceptedResponse>(command, ct));

    [HttpPut("{id:guid}")]
    [Authorize(Roles = RoleConstants.TimeUsers)]
    [SwaggerOperation(Summary = "Update a leave request.")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLeaveRequestCommand command, CancellationToken ct)
        => Ok(await _dispatcher.SendAsync<UpdateLeaveRequestCommand, CommandAcceptedResponse>(command with { Id = id }, ct));

    [HttpPost("{id:guid}/submit")]
    [Authorize(Roles = RoleConstants.TimeUsers)]
    [SwaggerOperation(Summary = "Submit a leave request.")]
    public async Task<IActionResult> Submit(Guid id, CancellationToken ct)
        => Ok(await _dispatcher.SendAsync<SubmitLeaveRequestCommand, CommandAcceptedResponse>(new SubmitLeaveRequestCommand(id), ct));

    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = RoleConstants.TimeReviewers)]
    [SwaggerOperation(Summary = "Approve a leave request.")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ReviewDecisionRequest request, CancellationToken ct)
        => Ok(await _dispatcher.SendAsync<ApproveLeaveRequestCommand, CommandAcceptedResponse>(new ApproveLeaveRequestCommand(id, request.Comment), ct));

    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = RoleConstants.TimeReviewers)]
    [SwaggerOperation(Summary = "Reject a leave request.")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] ReviewDecisionRequest request, CancellationToken ct)
        => Ok(await _dispatcher.SendAsync<RejectLeaveRequestCommand, CommandAcceptedResponse>(new RejectLeaveRequestCommand(id, request.Comment), ct));

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = RoleConstants.TimeUsers)]
    [SwaggerOperation(Summary = "Cancel a leave request.")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] ReviewDecisionRequest request, CancellationToken ct)
        => Ok(await _dispatcher.SendAsync<CancelLeaveRequestCommand, CommandAcceptedResponse>(new CancelLeaveRequestCommand(id, request.Comment), ct));
}
