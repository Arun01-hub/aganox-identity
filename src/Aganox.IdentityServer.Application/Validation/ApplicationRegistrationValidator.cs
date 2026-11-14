using Aganox.IdentityServer.Application.Models.Applications;
using Aganox.IdentityServer.Domain.Enums;

namespace Aganox.IdentityServer.Application.Validation;

public sealed class ApplicationRegistrationValidator(RedirectUriValidator redirectUriValidator)
{
    public void ValidateRegister(RegisterApplicationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            throw new ApplicationValidationException("Display name is required.");
        }

        ValidateTypeRules(
            request.ApplicationType,
            request.RedirectUris,
            request.PostLogoutRedirectUris,
            request.AllowedOrigins);

        redirectUriValidator.ValidateUris(request.Environment, request.RedirectUris, "Redirect URI");
        redirectUriValidator.ValidateUris(request.Environment, request.PostLogoutRedirectUris, "Post-logout redirect URI");
        redirectUriValidator.ValidateOrigins(request.AllowedOrigins);

        ValidateScopes(request.ApplicationType, request.Scopes);
    }

    public void ValidateTypeRules(
        ApplicationType type,
        IReadOnlyList<string> redirectUris,
        IReadOnlyList<string> postLogoutRedirectUris,
        IReadOnlyList<string> allowedOrigins)
    {
        switch (type)
        {
            case ApplicationType.SpaPublic:
                if (redirectUris.Count == 0)
                {
                    throw new ApplicationValidationException("SPA applications require at least one redirect URI.");
                }

                break;

            case ApplicationType.WebConfidential:
                if (redirectUris.Count == 0)
                {
                    throw new ApplicationValidationException("Web confidential applications require at least one redirect URI.");
                }

                break;

            case ApplicationType.ServiceM2m:
                if (redirectUris.Count > 0 || postLogoutRedirectUris.Count > 0 || allowedOrigins.Count > 0)
                {
                    throw new ApplicationValidationException("M2M applications cannot have redirect URIs or CORS origins.");
                }

                break;
        }
    }

    public void ValidateScopes(ApplicationType type, IReadOnlyList<string> scopes)
    {
        if (scopes.Count == 0)
        {
            throw new ApplicationValidationException("At least one scope is required.");
        }

        foreach (var scope in scopes)
        {
            if (!ScopeNames.All.Contains(scope, StringComparer.OrdinalIgnoreCase))
            {
                throw new ApplicationValidationException($"Scope '{scope}' is not allowed.");
            }
        }

        if (type == ApplicationType.ServiceM2m &&
            scopes.Any(scope => string.Equals(scope, ScopeNames.OpenId, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ApplicationValidationException("M2M applications cannot request the openid scope.");
        }
    }
}
