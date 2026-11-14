using Aganox.IdentityServer.Application.Abstractions;
using Aganox.IdentityServer.Application.Models.Applications;
using Aganox.IdentityServer.Application.Validation;
using Aganox.IdentityServer.Domain.Constants;
using Aganox.IdentityServer.Domain.Enums;
using Aganox.IdentityServer.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aganox.IdentityServer.Controllers.Admin;

[ApiController]
[Route("api/admin/applications")]
[Authorize(Policy = PolicyNames.RequireAdministrator)]
public sealed class ApplicationsController(IApplicationRegistrationService registrationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ApplicationDetailDto>>> List(
        [FromQuery] ApplicationStatus? status,
        [FromQuery] DeploymentEnvironment? environment,
        [FromQuery] ApplicationType? applicationType,
        [FromQuery] string? search,
        [FromQuery] bool includeArchived = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await registrationService.ListAsync(new ApplicationListQuery
        {
            Status = status,
            Environment = environment,
            ApplicationType = applicationType,
            Search = search,
            IncludeArchived = includeArchived,
            Page = page,
            PageSize = pageSize
        }, cancellationToken);

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<RegisterApplicationResult>> Register(
        [FromBody] RegisterApplicationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await registrationService.RegisterAsync(
                request,
                AdminActorHelper.FromHttpContext(HttpContext),
                cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = result.Application.Id }, result);
        }
        catch (ApplicationValidationException ex)
        {
            return BadRequest(new { errors = ex.Errors.Count > 0 ? ex.Errors : new[] { ex.Message } });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApplicationDetailDto>> Get(Guid id, CancellationToken cancellationToken = default)
    {
        var application = await registrationService.GetAsync(id, cancellationToken);
        return application is null ? NotFound() : Ok(application);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApplicationDetailDto>> UpdateMetadata(
        Guid id,
        [FromBody] UpdateMetadataRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await registrationService.UpdateMetadataAsync(
                id,
                request.DisplayName,
                request.Description,
                AdminActorHelper.FromHttpContext(HttpContext),
                cancellationToken);
            return Ok(result);
        }
        catch (ApplicationValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}/redirect-uris")]
    public async Task<ActionResult<ApplicationDetailDto>> UpdateRedirectUris(
        Guid id,
        [FromBody] UpdateRedirectUrisRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await registrationService.UpdateRedirectUrisAsync(
                id,
                request.RedirectUris,
                request.PostLogoutRedirectUris,
                AdminActorHelper.FromHttpContext(HttpContext),
                cancellationToken);
            return Ok(result);
        }
        catch (ApplicationValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}/origins")]
    public async Task<ActionResult<ApplicationDetailDto>> UpdateOrigins(
        Guid id,
        [FromBody] UpdateOriginsRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await registrationService.UpdateOriginsAsync(
                id,
                request.AllowedOrigins,
                AdminActorHelper.FromHttpContext(HttpContext),
                cancellationToken);
            return Ok(result);
        }
        catch (ApplicationValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}/scopes")]
    public async Task<ActionResult<ApplicationDetailDto>> UpdateScopes(
        Guid id,
        [FromBody] UpdateScopesRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await registrationService.UpdateScopesAsync(
                id,
                request.Scopes,
                AdminActorHelper.FromHttpContext(HttpContext),
                cancellationToken);
            return Ok(result);
        }
        catch (ApplicationValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id:guid}/status")]
    public async Task<ActionResult<ApplicationDetailDto>> ChangeStatus(
        Guid id,
        [FromBody] ChangeStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await registrationService.ChangeStatusAsync(
                id,
                request.Status,
                AdminActorHelper.FromHttpContext(HttpContext),
                cancellationToken);
            return Ok(result);
        }
        catch (ApplicationValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id:guid}/credentials/rotate")]
    public async Task<ActionResult<RotateSecretResult>> RotateSecret(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await registrationService.RotateSecretAsync(
                id,
                AdminActorHelper.FromHttpContext(HttpContext),
                cancellationToken);
            return Ok(result);
        }
        catch (ApplicationValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id:guid}/credentials/revoke")]
    public async Task<ActionResult<ApplicationDetailDto>> RevokeSecret(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await registrationService.RevokeSecretAsync(
                id,
                AdminActorHelper.FromHttpContext(HttpContext),
                cancellationToken);
            return Ok(result);
        }
        catch (ApplicationValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id:guid}/audit")]
    public async Task<ActionResult<PagedResult<ApplicationAuditDto>>> GetAudit(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await registrationService.GetAuditAsync(id, page, pageSize, cancellationToken);
            return Ok(result);
        }
        catch (ApplicationValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

public sealed class UpdateMetadataRequest
{
    public required string DisplayName { get; init; }

    public string? Description { get; init; }
}

public sealed class UpdateRedirectUrisRequest
{
    public IReadOnlyList<string> RedirectUris { get; init; } = [];

    public IReadOnlyList<string> PostLogoutRedirectUris { get; init; } = [];
}

public sealed class UpdateOriginsRequest
{
    public IReadOnlyList<string> AllowedOrigins { get; init; } = [];
}

public sealed class UpdateScopesRequest
{
    public IReadOnlyList<string> Scopes { get; init; } = [];
}

public sealed class ChangeStatusRequest
{
    public ApplicationStatus Status { get; init; }
}
