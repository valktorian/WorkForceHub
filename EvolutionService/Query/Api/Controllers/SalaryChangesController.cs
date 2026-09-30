using EvolutionService.Query.Domain;
using EvolutionService.Query.Domain.Repositories;
using EvolutionService.Query.Infrastructure;
using Infrastructure.Api.Common;
using Infrastructure.Api.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace EvolutionService.Query.Api.Controllers;

[ApiController]
[Route("api/salary-changes")]
[Authorize]
public class SalaryChangesController : ControllerBase
{
    private readonly IEvolutionReadRepository<SalaryChangeReadModel> _repository;
    private readonly ReadDbContext _readDbContext;

    public SalaryChangesController(IEvolutionReadRepository<SalaryChangeReadModel> repository, ReadDbContext readDbContext)
    {
        _repository = repository;
        _readDbContext = readDbContext;
    }

    [HttpGet]
    [Authorize(Roles = RoleConstants.EvolutionUsers)]
    [SwaggerOperation(Summary = "List salary changes.")]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest pagination, CancellationToken ct)
    {
        var totalCount = await _repository.CountAsync(ct);
        var items = await _repository.GetPagedAsync(pagination.Skip, pagination.NormalizedPageSize, ct);
        return Ok(BaseResponse<PagedResult<SalaryChangeReadModel>>.Ok(PagedResult<SalaryChangeReadModel>.Create(items, pagination.NormalizedPageNumber, pagination.NormalizedPageSize, totalCount)));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = RoleConstants.EvolutionUsers)]
    [SwaggerOperation(Summary = "Get a salary change by ID.")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var item = await _repository.GetByIdAsync(id, ct);
        return item is null ? NotFound(BaseResponse<object>.Fail("Salary change not found.")) : Ok(BaseResponse<SalaryChangeReadModel>.Ok(item));
    }

    [HttpGet("employee/{employeeId:guid}")]
    [Authorize(Roles = RoleConstants.EvolutionUsers)]
    [SwaggerOperation(Summary = "List salary changes for an employee.")]
    public async Task<IActionResult> GetByEmployee(Guid employeeId, CancellationToken ct)
        => Ok(BaseResponse<IReadOnlyList<SalaryChangeReadModel>>.Ok(await _repository.GetByEmployeeAsync(employeeId, ct)));

    [HttpGet("self")]
    [SwaggerOperation(Summary = "List salary changes for the current user.")]
    public async Task<IActionResult> GetSelf(CancellationToken ct)
    {
        var employeeId = await ResolveCurrentEmployeeIdAsync(ct);
        if (employeeId is null)
        {
            return NotFound(BaseResponse<object>.Fail("Profile not found for current account."));
        }

        return Ok(BaseResponse<IReadOnlyList<SalaryChangeReadModel>>.Ok(await _repository.GetByEmployeeAsync(employeeId.Value, ct)));
    }

    private async Task<Guid?> ResolveCurrentEmployeeIdAsync(CancellationToken ct)
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(value, out var accountId))
        {
            return null;
        }

        var profile = await _readDbContext.Profiles.Find(x => x.AccountId == accountId).FirstOrDefaultAsync(ct);
        return profile?.Id;
    }
}
