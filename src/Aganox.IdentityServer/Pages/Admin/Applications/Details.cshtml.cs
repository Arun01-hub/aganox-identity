using Aganox.IdentityServer.Application.Abstractions;
using Aganox.IdentityServer.Application.Models.Applications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Aganox.IdentityServer.Pages.Admin.Applications;

[Authorize(Roles = "Administrator")]
public sealed class DetailsModel(IApplicationRegistrationService registrationService) : PageModel
{
    public ApplicationDetailDto Application { get; private set; } = null!;

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
}
