using Aganox.IdentityServer.Application.Abstractions;
using Aganox.IdentityServer.Application.Models.Applications;
using Aganox.IdentityServer.Application.Validation;
using Aganox.IdentityServer.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Aganox.IdentityServer.Pages.Admin.Applications;

[Authorize(Roles = "Administrator")]
public sealed class EditModel(IApplicationRegistrationService registrationService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

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
        Input = InputModel.From(application);
        return Page();
    }

    public async Task<IActionResult> OnPostMetadataAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            Application = await registrationService.UpdateMetadataAsync(
                id,
                Input.DisplayName,
                Input.Description,
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

    public async Task<IActionResult> OnPostUrisAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            Application = await registrationService.UpdateRedirectUrisAsync(
                id,
                SplitLines(Input.RedirectUris),
                SplitLines(Input.PostLogoutRedirectUris),
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

    public async Task<IActionResult> OnPostOriginsAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            Application = await registrationService.UpdateOriginsAsync(
                id,
                SplitLines(Input.AllowedOrigins),
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

    public async Task<IActionResult> OnPostScopesAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            Application = await registrationService.UpdateScopesAsync(
                id,
                SplitLines(Input.Scopes),
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

    private static IReadOnlyList<string> SplitLines(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    public sealed class InputModel
    {
        public string DisplayName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? RedirectUris { get; set; }

        public string? PostLogoutRedirectUris { get; set; }

        public string? AllowedOrigins { get; set; }

        public string Scopes { get; set; } = string.Empty;

        public static InputModel From(ApplicationDetailDto application) => new()
        {
            DisplayName = application.DisplayName,
            Description = application.Description,
            RedirectUris = string.Join(Environment.NewLine, application.RedirectUris),
            PostLogoutRedirectUris = string.Join(Environment.NewLine, application.PostLogoutRedirectUris),
            AllowedOrigins = string.Join(Environment.NewLine, application.AllowedOrigins),
            Scopes = string.Join(Environment.NewLine, application.Scopes)
        };
    }
}
