using Aganox.IdentityServer.Application.Abstractions;
using Aganox.IdentityServer.Application.Models.Applications;
using Aganox.IdentityServer.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Aganox.IdentityServer.Pages.Admin.Applications;

[Authorize(Roles = "Administrator")]
public sealed class IndexModel(IApplicationRegistrationService registrationService) : PageModel
{
    public PagedResult<ApplicationDetailDto> Applications { get; private set; } = new()
    {
        Items = [],
        TotalCount = 0,
        Page = 1,
        PageSize = 20
    };

    [BindProperty(SupportsGet = true)]
    public ApplicationStatus? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public DeploymentEnvironment? Environment { get; set; }

    [BindProperty(SupportsGet = true)]
    public ApplicationType? ApplicationType { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool IncludeArchived { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Applications = await registrationService.ListAsync(new ApplicationListQuery
        {
            Status = Status,
            Environment = Environment,
            ApplicationType = ApplicationType,
            Search = Search,
            IncludeArchived = IncludeArchived,
            Page = PageNumber,
            PageSize = 20
        }, cancellationToken);
    }
}
