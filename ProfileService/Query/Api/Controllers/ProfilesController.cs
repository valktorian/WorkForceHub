using Infrastructure.Api.Common;
using Infrastructure.Api.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using ProfileService.Query.Domain;
using ProfileService.Query.Infrastructure;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace ProfileService.Query.Api.Controllers;

[ApiController]
[Route("api/profiles")]
[Authorize]
public class ProfilesController : ControllerBase
{
    private readonly ReadDbContext _readDb;
    private readonly ILogger<ProfilesController> _logger;

    public ProfilesController(ReadDbContext readDb, ILogger<ProfilesController> logger)
    {
        _readDb = readDb;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = RoleConstants.HrStaff)]
    [SwaggerOperation(Summary = "List profiles from the read model.")]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest pagination, CancellationToken ct)
    {
        try
        {
            var totalCount = await _readDb.Profiles.CountDocumentsAsync(FilterDefinition<ProfileReadModel>.Empty, cancellationToken: ct);
            var profiles = await _readDb.Profiles
                .Find(_ => true)
                .SortByDescending(x => x.CreatedAt)
                .Skip(pagination.Skip)
                .Limit(pagination.NormalizedPageSize)
                .ToListAsync(ct);

            var result = PagedResult<ProfileReadModel>.Create(
                profiles,
                pagination.NormalizedPageNumber,
                pagination.NormalizedPageSize,
                totalCount);

            return Ok(BaseResponse<PagedResult<ProfileReadModel>>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching profiles.");
            return StatusCode(500, BaseResponse<object>.Fail("Error fetching profiles."));
        }
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = RoleConstants.HrStaff)]
    [SwaggerOperation(Summary = "Get a profile by ID.")]
    public Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => FindOne(x => x.Id == id, "Profile not found.", ct);

    [HttpGet("by-account/{accountId:guid}")]
    [Authorize(Roles = RoleConstants.HrStaff)]
    [SwaggerOperation(Summary = "Get a profile by account ID.")]
    public Task<IActionResult> GetByAccount(Guid accountId, CancellationToken ct)
        => FindOne(x => x.AccountId == accountId, "Profile not found for account.", ct);

    [HttpGet("self")]
    [SwaggerOperation(Summary = "Get the current user's profile.")]
    public Task<IActionResult> GetSelf(CancellationToken ct)
    {
        var accountIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(accountIdValue, out var accountId))
        {
            return Task.FromResult<IActionResult>(Unauthorized(BaseResponse<object>.Fail("Authenticated account identifier is missing.")));
        }

        return FindOne(x => x.AccountId == accountId, "Profile not found for current account.", ct);
    }

    private async Task<IActionResult> FindOne(
        System.Linq.Expressions.Expression<Func<ProfileReadModel, bool>> predicate,
        string notFoundMessage,
        CancellationToken ct)
    {
        try
        {
            var profile = await _readDb.Profiles.Find(predicate).FirstOrDefaultAsync(ct);
            if (profile is null)
            {
                return NotFound(BaseResponse<object>.Fail(notFoundMessage));
            }

            return Ok(BaseResponse<ProfileReadModel>.Ok(profile));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching profile.");
            return StatusCode(500, BaseResponse<object>.Fail("Error fetching profile."));
        }
    }
}
