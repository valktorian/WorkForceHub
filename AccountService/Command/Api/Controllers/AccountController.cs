using AccountService.Command.Application.Commands;
using AccountService.Command.Application.DTOs;
using Infrastructure.Api.Constants;
using Infrastructure.Api.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AccountService.Command.Api.Controllers;

[ApiController]
[Route("api/accounts")]
[Authorize(Roles = RoleConstants.HrAdmin)]
public class AccountController : ControllerBase
{
    private readonly ICommandDispatcher _dispatcher;

    public AccountController(ICommandDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    /// <summary>
    /// Create a new account on the command side and publish the account-created event.
    /// </summary>
    /// <param name="command">The account payload to create.</param>
    /// <param name="ct">The request cancellation token.</param>
    /// <returns>The created account response.</returns>
    [HttpPost]
    [SwaggerOperation(Summary = "Create a new account.")]
    [ProducesResponseType(typeof(CreateAccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateAccountCommand command, CancellationToken ct)
    {
        var response = await _dispatcher.SendAsync<CreateAccountCommand, CreateAccountResponse>(command, ct);
        return Ok(response);
    }

    /// <summary>
    /// Update the main profile information of an existing account.
    /// </summary>
    /// <param name="id">The account identifier.</param>
    /// <param name="command">The new account profile values.</param>
    /// <param name="ct">The request cancellation token.</param>
    /// <returns>The updated account response.</returns>
    [HttpPut("{id:guid}")]
    [SwaggerOperation(Summary = "Update an account profile.")]
    [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAccountCommand command, CancellationToken ct)
    {
        var response = await _dispatcher.SendAsync<UpdateAccountCommand, AccountResponse>(
            command with { AccountId = id },
            ct);

        return Ok(response);
    }

    /// <summary>
    /// Change the role of an existing account.
    /// </summary>
    /// <param name="id">The account identifier.</param>
    /// <param name="command">The role update payload.</param>
    /// <param name="ct">The request cancellation token.</param>
    /// <returns>The updated account response.</returns>
    [HttpPatch("{id:guid}/role")]
    [SwaggerOperation(Summary = "Update an account role.")]
    [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateAccountRoleCommand command, CancellationToken ct)
    {
        var response = await _dispatcher.SendAsync<UpdateAccountRoleCommand, AccountResponse>(
            command with { AccountId = id },
            ct);

        return Ok(response);
    }

    /// <summary>
    /// Change the password hash of an existing account.
    /// </summary>
    /// <param name="id">The account identifier.</param>
    /// <param name="command">The password change payload.</param>
    /// <param name="ct">The request cancellation token.</param>
    [HttpPatch("{id:guid}/password")]
    [SwaggerOperation(Summary = "Change an account password.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePassword(Guid id, [FromBody] ChangeAccountPasswordCommand command, CancellationToken ct)
    {
        await _dispatcher.SendAsync<ChangeAccountPasswordCommand, bool>(
            command with { AccountId = id },
            ct);

        return NoContent();
    }

    /// <summary>
    /// Delete an account and publish the account-deleted event.
    /// </summary>
    /// <param name="id">The account identifier.</param>
    /// <param name="ct">The request cancellation token.</param>
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(Summary = "Delete an account.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _dispatcher.SendAsync<DeleteAccountCommand, bool>(new DeleteAccountCommand(id), ct);
        return NoContent();
    }
}
