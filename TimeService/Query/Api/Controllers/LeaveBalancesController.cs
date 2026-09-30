using Infrastructure.Api.Common;
using Infrastructure.Api.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using TimeService.Query.Domain;
using TimeService.Query.Domain.Repositories;

namespace TimeService.Query.Api.Controllers;

[ApiController]
[Route("api/leave-balances")]
[Authorize]
public class LeaveBalancesController : ControllerBase
{
    private readonly ILeaveBalanceReadRepository _repository;

    public LeaveBalancesController(ILeaveBalanceReadRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("{employeeId:guid}")]
    [Authorize(Roles = RoleConstants.TimeReviewers)]
    [SwaggerOperation(Summary = "List leave balances for an employee.")]
    public async Task<IActionResult> GetByEmployee(Guid employeeId, CancellationToken ct)
        => Ok(BaseResponse<IReadOnlyList<LeaveBalanceReadModel>>.Ok(await _repository.GetByEmployeeAsync(employeeId, ct)));

    [HttpGet("self")]
    [SwaggerOperation(Summary = "List leave balances for the current user.")]
    public async Task<IActionResult> GetSelf(CancellationToken ct)
        => Ok(BaseResponse<IReadOnlyList<LeaveBalanceReadModel>>.Ok(await _repository.GetByAccountAsync(GetCurrentAccountId(), ct)));

    private Guid GetCurrentAccountId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(value, out var id) ? id : Guid.Empty;
    }
}
