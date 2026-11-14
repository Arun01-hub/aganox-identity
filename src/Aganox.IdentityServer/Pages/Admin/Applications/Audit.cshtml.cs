using Aganox.IdentityServer.Application.Abstractions;
using Aganox.IdentityServer.Application.Models.Applications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Aganox.IdentityServer.Pages.Admin.Applications;

[Authorize(Roles = "Administrator")]
public sealed class AuditModel(IApplicationRegistrationService registrationService) : PageModel
{
    public ApplicationDetailDto Application { get; private set; } = null!;

    public PagedResult<ApplicationAuditDto> AuditEntries { get; private set; } = new()
    {
        Items = [],
        TotalCount = 0,
        Page = 1,
        PageSize = 50
    };

    public async Task<IActionResult> OnGetAsync(Guid id, int page = 1, CancellationToken cancellationToken = default)
    {
        var application = await registrationService.GetAsync(id, cancellationToken);
        if (application is null)
        {
            return NotFound();
        }

        Application = application;
        AuditEntries = await registrationService.GetAuditAsync(id, page, 50, cancellationToken);
        return Page();
    }
}
