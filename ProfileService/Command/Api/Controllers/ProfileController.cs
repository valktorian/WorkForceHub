using Infrastructure.Api.Authentication;
using Infrastructure.Api.Common;
using Infrastructure.Api.Constants;
using Infrastructure.Api.Messaging;
using Infrastructure.Api.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProfileService.Command.Api.Models;
using ProfileService.Command.Application.Commands;
using ProfileService.Command.Application.DTOs;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json;

namespace ProfileService.Command.Api.Controllers;

[ApiController]
[Route("api/profiles")]
[Authorize]
public class ProfileController : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ICommandDispatcher _dispatcher;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IExternalFileStorageClient _fileStorageClient;

    public ProfileController(ICommandDispatcher dispatcher, ICurrentUserAccessor currentUserAccessor, IExternalFileStorageClient fileStorageClient)
    {
        _dispatcher = dispatcher;
        _currentUserAccessor = currentUserAccessor;
        _fileStorageClient = fileStorageClient;
    }

    [HttpPost]
    [Authorize(Roles = RoleConstants.HrStaff)]
    [Consumes("application/json")]
    [SwaggerOperation(Summary = "Create a profile.")]
    public async Task<IActionResult> Create([FromBody] CreateProfileCommand command, CancellationToken ct)
    {
        var response = await _dispatcher.SendAsync<CreateProfileCommand, ProfileResponse>(command, ct);
        return Ok(response);
    }

    [HttpPost("with-picture")]
    [Authorize(Roles = RoleConstants.HrStaff)]
    [Consumes("multipart/form-data")]
    [SwaggerOperation(Summary = "Create a profile with a picture.")]
    public async Task<IActionResult> CreateWithPicture([FromForm] CreateProfileFormRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Payload))
        {
            throw ApiException.BadRequest("The 'payload' form field is required.");
        }

        CreateProfileCommand? command;

        try
        {
            command = JsonSerializer.Deserialize<CreateProfileCommand>(request.Payload, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw ApiException.BadRequest("The 'payload' form field contains invalid JSON.", ex.Message);
        }

        if (command is null)
        {
            throw ApiException.BadRequest("The 'payload' form field contains invalid JSON.");
        }

        var profilePictureUrl = request.ProfilePicture is null
            ? null
            : await UploadPictureAsync(request.ProfilePicture, ct);

        var response = await _dispatcher.SendAsync<CreateProfileCommand, ProfileResponse>(
            command with { ProfilePictureUrl = profilePictureUrl },
            ct);

        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = RoleConstants.HrStaff)]
    [SwaggerOperation(Summary = "Update a profile.")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProfileCommand command, CancellationToken ct)
    {
        var response = await _dispatcher.SendAsync<UpdateProfileCommand, ProfileResponse>(command with { ProfileId = id }, ct);
        return Ok(response);
    }

    [HttpPatch("{id:guid}/employment")]
    [Authorize(Roles = RoleConstants.HrStaff)]
    [SwaggerOperation(Summary = "Update profile employment details.")]
    public async Task<IActionResult> UpdateEmployment(Guid id, [FromBody] UpdateProfileEmploymentCommand command, CancellationToken ct)
    {
        var response = await _dispatcher.SendAsync<UpdateProfileEmploymentCommand, ProfileResponse>(command with { ProfileId = id }, ct);
        return Ok(response);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = RoleConstants.HrStaff)]
    [SwaggerOperation(Summary = "Update a profile employment status.")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateProfileStatusCommand command, CancellationToken ct)
    {
        var response = await _dispatcher.SendAsync<UpdateProfileStatusCommand, ProfileResponse>(command with { ProfileId = id }, ct);
        return Ok(response);
    }

    [HttpPost("{id:guid}/link-account")]
    [Authorize(Roles = RoleConstants.HrStaff)]
    [SwaggerOperation(Summary = "Link a profile to an account.")]
    public async Task<IActionResult> LinkAccount(Guid id, [FromBody] LinkProfileAccountCommand command, CancellationToken ct)
    {
        var response = await _dispatcher.SendAsync<LinkProfileAccountCommand, ProfileResponse>(command with { ProfileId = id }, ct);
        return Ok(response);
    }

    [HttpPatch("self/personal-info")]
    [SwaggerOperation(Summary = "Update personal information for the current profile.")]
    public async Task<IActionResult> UpdateSelfPersonalInfo([FromBody] UpdateSelfPersonalInfoCommand command, CancellationToken ct)
    {
        var response = await _dispatcher.SendAsync<UpdateSelfPersonalInfoCommand, ProfileResponse>(
            command with { AccountId = _currentUserAccessor.GetRequiredAccountId() },
            ct);

        return Ok(response);
    }

    [HttpPost("{id:guid}/picture")]
    [Authorize(Roles = RoleConstants.HrStaff)]
    [SwaggerOperation(Summary = "Upload a profile picture.")]
    public async Task<IActionResult> UploadProfilePicture(Guid id, IFormFile? file, CancellationToken ct)
    {
        var response = await UploadAndUpdatePictureAsync(
            file,
            new UpdateProfilePictureCommand(id, null, string.Empty),
            ct);

        return Ok(response);
    }

    [HttpPost("self/picture")]
    [SwaggerOperation(Summary = "Upload a picture for the current profile.")]
    public async Task<IActionResult> UploadSelfProfilePicture(IFormFile? file, CancellationToken ct)
    {
        var response = await UploadAndUpdatePictureAsync(
            file,
            new UpdateProfilePictureCommand(null, _currentUserAccessor.GetRequiredAccountId(), string.Empty),
            ct);

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = RoleConstants.HrStaff)]
    [SwaggerOperation(Summary = "Delete a profile.")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _dispatcher.SendAsync<DeleteProfileCommand, bool>(new DeleteProfileCommand(id), ct);
        return NoContent();
    }

    private async Task<ProfileResponse> UploadAndUpdatePictureAsync(
        IFormFile? file,
        UpdateProfilePictureCommand command,
        CancellationToken ct)
    {
        if (file is null || file.Length == 0)
        {
            throw global::Infrastructure.Api.Common.ApiException.BadRequest("A non-empty image file is required.");
        }

        var uploadedFileUrl = await UploadPictureAsync(file, ct);

        return await _dispatcher.SendAsync<UpdateProfilePictureCommand, ProfileResponse>(
            command with { ProfilePictureUrl = uploadedFileUrl },
            ct);
    }

    private async Task<string> UploadPictureAsync(IFormFile file, CancellationToken ct)
    {
        await using var stream = file.OpenReadStream();
        var uploadedFile = await _fileStorageClient.UploadImageAsync(
            stream,
            file.FileName,
            file.ContentType,
            "profile-pictures",
            ct);

        return uploadedFile.Url;
    }
}
