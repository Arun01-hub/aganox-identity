using Aganox.IdentityServer.Domain.Entities;
using Aganox.IdentityServer.Domain.Enums;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Aganox.IdentityServer.Infrastructure.OpenIddict;

internal static class OpenIddictPermissionMapper
{
    public static IReadOnlyList<string> BuildPermissions(RegisteredApplication application, bool enabled)
    {
        if (!enabled)
        {
            return [];
        }

        var scopes = application.Scopes.Select(scope => scope.Scope).ToList();
        var permissions = new List<string>();

        switch (application.ApplicationType)
        {
            case ApplicationType.SpaPublic:
            case ApplicationType.WebConfidential:
                permissions.Add(Permissions.Endpoints.Authorization);
                permissions.Add(Permissions.Endpoints.Token);
                permissions.Add(Permissions.Endpoints.EndSession);
                permissions.Add(Permissions.GrantTypes.AuthorizationCode);
                permissions.Add(Permissions.GrantTypes.RefreshToken);
                permissions.Add(Permissions.ResponseTypes.Code);
                permissions.Add(Permissions.Scopes.Email);
                permissions.Add(Permissions.Scopes.Profile);
                permissions.Add(Permissions.Scopes.Roles);
                break;

            case ApplicationType.ServiceM2m:
                permissions.Add(Permissions.Endpoints.Token);
                permissions.Add(Permissions.GrantTypes.ClientCredentials);
                break;
        }

        foreach (var scope in scopes)
        {
            if (string.Equals(scope, ScopeNames.OpenId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            permissions.Add(Permissions.Prefixes.Scope + scope);
        }

        return permissions;
    }
}
