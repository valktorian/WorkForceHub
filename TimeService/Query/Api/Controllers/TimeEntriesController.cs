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
[Route("api/time-entries")]
[Authorize]
public class TimeEntriesController : ControllerBase
{
    private readonly ITimeEntryReadRepository _repository;

    public TimeEntriesController(ITimeEntryReadRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Get a time entry by ID.")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var item = await _repository.GetByIdAsync(id, ct);
        return item is null
            ? NotFound(BaseResponse<object>.Fail("Time entry not found."))
            : Ok(BaseResponse<TimeEntryReadModel>.Ok(item));
    }

    [HttpGet]
    [Authorize(Roles = RoleConstants.TimeReviewers)]
    [SwaggerOperation(Summary = "List time entries.")]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest pagination, CancellationToken ct)
    {
        var totalCount = await _repository.CountAsync(ct);
        var items = await _repository.GetPagedAsync(pagination.Skip, pagination.NormalizedPageSize, ct);
        var result = PagedResult<TimeEntryReadModel>.Create(items, pagination.NormalizedPageNumber, pagination.NormalizedPageSize, totalCount);
        return Ok(BaseResponse<PagedResult<TimeEntryReadModel>>.Ok(result));
    }

    [HttpGet("by-employee/{employeeId:guid}")]
    [Authorize(Roles = RoleConstants.TimeReviewers)]
    [SwaggerOperation(Summary = "List time entries for an employee.")]
    public async Task<IActionResult> GetByEmployee(Guid employeeId, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken ct)
        => Ok(BaseResponse<IReadOnlyList<TimeEntryReadModel>>.Ok(await _repository.GetByEmployeeAsync(employeeId, from, to, ct)));

    [HttpGet("self")]
    [SwaggerOperation(Summary = "List time entries for the current user.")]
    public async Task<IActionResult> GetSelf([FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken ct)
        => Ok(BaseResponse<IReadOnlyList<TimeEntryReadModel>>.Ok(await _repository.GetByAccountAsync(GetCurrentAccountId(), from, to, ct)));

    private Guid GetCurrentAccountId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(value, out var id) ? id : Guid.Empty;
    }
}
