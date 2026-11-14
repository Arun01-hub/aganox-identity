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
public sealed class LifecycleModel(IApplicationRegistrationService registrationService) : PageModel
{
    [BindProperty]
    public ApplicationStatus TargetStatus { get; set; }

    public ApplicationDetailDto Application { get; private set; } = null!;

    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var application = await registrationService.GetAsync(id, cancellationToken);
        if (application is null)
        {
            return NotFound();
        }

        Application = application;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await registrationService.ChangeStatusAsync(
                id,
                TargetStatus,
                AdminActorHelper.FromHttpContext(HttpContext),
                cancellationToken);
            return RedirectToPage("Details", new { id });
        }
        catch (ApplicationValidationException ex)
        {
            ErrorMessage = ex.Message;
            Application = (await registrationService.GetAsync(id, cancellationToken))!;
            return Page();
        }
    }
}
