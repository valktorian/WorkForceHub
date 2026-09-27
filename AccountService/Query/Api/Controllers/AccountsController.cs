using AccountService.Query.Domain;
using AccountService.Query.Infrastructure;
using Infrastructure.Api.Common;
using Infrastructure.Api.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Swashbuckle.AspNetCore.Annotations;

namespace AccountService.Query.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = RoleConstants.HrAdmin)]
public class AccountsController : ControllerBase
{
    private readonly ReadDbContext _readDb;
    private readonly ILogger<AccountsController> _logger;

    public AccountsController(ReadDbContext readDb, ILogger<AccountsController> logger)
    {
        _readDb = readDb;
        _logger = logger;
    }

    /// <summary>
    /// Get all accounts from MongoDB read model
    /// </summary>
    [HttpGet]
    [SwaggerOperation(Summary = "List accounts from the read model.")]
    [ProducesResponseType(typeof(BaseResponse<PagedResult<AccountReadModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllAccounts([FromQuery] PaginationRequest pagination, CancellationToken ct)
    {
        try
        {
            var totalCount = await _readDb.Accounts.CountDocumentsAsync(FilterDefinition<AccountReadModel>.Empty, cancellationToken: ct);
            var accounts = await _readDb.Accounts
                .Find(_ => true)
                .SortByDescending(x => x.CreatedAt)
                .Skip(pagination.Skip)
                .Limit(pagination.NormalizedPageSize)
                .ToListAsync(ct);

            var result = PagedResult<AccountReadModel>.Create(
                accounts,
                pagination.NormalizedPageNumber,
                pagination.NormalizedPageSize,
                totalCount);

            return Ok(BaseResponse<PagedResult<AccountReadModel>>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching accounts");
            return StatusCode(500, BaseResponse<object>.Fail("Error fetching accounts"));
        }
    }

    /// <summary>
    /// Get account by ID
    /// </summary>
    /// <param name="id">The account identifier.</param>
    [HttpGet("{id}", Name = "GetAccountById")]
    [SwaggerOperation(Summary = "Get an account by ID.")]
    [ProducesResponseType(typeof(BaseResponse<AccountReadModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAccountById(Guid id)
    {
        try
        {
            var account = await _readDb.Accounts.Find(a => a.Id == id).FirstOrDefaultAsync();

            if (account == null)
                return NotFound(BaseResponse<object>.Fail("Account not found"));

            return Ok(BaseResponse<AccountReadModel>.Ok(account));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching account");
            return StatusCode(500, BaseResponse<object>.Fail("Error fetching account"));
        }
    }
}
