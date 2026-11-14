using Aganox.IdentityServer.Application.Abstractions;
using Aganox.IdentityServer.Application.Models.Applications;
using Aganox.IdentityServer.Application.Validation;
using Aganox.IdentityServer.Domain.Enums;
using Aganox.IdentityServer.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Aganox.IdentityServer.Pages.Admin.Applications;

[Authorize(Roles = "Administrator")]
public sealed class CredentialsModel(IApplicationRegistrationService registrationService) : PageModel
{
    public ApplicationDetailDto Application { get; private set; } = null!;

    public string? PlaintextSecret { get; private set; }

    public string? ErrorMessage { get; private set; }

    public bool SupportsSecrets { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, bool created, CancellationToken cancellationToken)
    {
        var application = await LoadAsync(id, cancellationToken);
        if (application is null)
        {
            return NotFound();
        }

        if (created && TempData["PlaintextSecret"] is string secret)
        {
            PlaintextSecret = secret;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostRotateAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await registrationService.RotateSecretAsync(
                id,
                AdminActorHelper.FromHttpContext(HttpContext),
                cancellationToken);
            PlaintextSecret = result.PlaintextSecret;
            Application = result.Application;
            SupportsSecrets = true;
            return Page();
        }
        catch (ApplicationValidationException ex)
        {
            ErrorMessage = ex.Message;
            await LoadAsync(id, cancellationToken);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostRevokeAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            Application = await registrationService.RevokeSecretAsync(
                id,
                AdminActorHelper.FromHttpContext(HttpContext),
                cancellationToken);
            SupportsSecrets = Application.ApplicationType is ApplicationType.WebConfidential or ApplicationType.ServiceM2m;
            return Page();
        }
        catch (ApplicationValidationException ex)
        {
            ErrorMessage = ex.Message;
            await LoadAsync(id, cancellationToken);
            return Page();
        }
    }

    private async Task<ApplicationDetailDto?> LoadAsync(Guid id, CancellationToken cancellationToken)
    {
        var application = await registrationService.GetAsync(id, cancellationToken);
        if (application is null)
        {
            return null;
        }

        Application = application;
        SupportsSecrets = application.ApplicationType is ApplicationType.WebConfidential or ApplicationType.ServiceM2m;
        return application;
    }
}
