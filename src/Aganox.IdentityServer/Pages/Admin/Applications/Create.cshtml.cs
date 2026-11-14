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
public sealed class CreateModel(IApplicationRegistrationService registrationService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        try
        {
            var result = await registrationService.RegisterAsync(new RegisterApplicationRequest
            {
                DisplayName = Input.DisplayName,
                Description = Input.Description,
                ApplicationType = Input.ApplicationType,
                Environment = Input.Environment,
                Slug = Input.Slug,
                ClientId = Input.ClientId,
                RedirectUris = SplitLines(Input.RedirectUris),
                PostLogoutRedirectUris = SplitLines(Input.PostLogoutRedirectUris),
                AllowedOrigins = SplitLines(Input.AllowedOrigins),
                Scopes = SplitLines(Input.Scopes),
                ActivateImmediately = Input.ActivateImmediately
            }, AdminActorHelper.FromHttpContext(HttpContext), cancellationToken);

            if (!string.IsNullOrEmpty(result.PlaintextSecret))
            {
                TempData["PlaintextSecret"] = result.PlaintextSecret;
                return RedirectToPage("Credentials", new { id = result.Application.Id, created = true });
            }

            return RedirectToPage("Details", new { id = result.Application.Id });
        }
        catch (ApplicationValidationException ex)
        {
            ErrorMessage = ex.Message;
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

        public ApplicationType ApplicationType { get; set; } = ApplicationType.SpaPublic;

        public DeploymentEnvironment Environment { get; set; } = DeploymentEnvironment.Development;

        public string? Slug { get; set; }

        public string? ClientId { get; set; }

        public string? RedirectUris { get; set; }

        public string? PostLogoutRedirectUris { get; set; }

        public string? AllowedOrigins { get; set; }

        public string Scopes { get; set; } = "openid\nprofile\nemail\noffline_access\naganox.api";

        public bool ActivateImmediately { get; set; } = true;
    }
}
