using Aganox.IdentityServer.Application.Abstractions;
using Aganox.IdentityServer.Domain.Entities;
using Aganox.IdentityServer.Domain.Enums;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Aganox.IdentityServer.Infrastructure.Seeding;

public sealed class OpenIddictLegacyImportService(
    IOpenIddictApplicationManager applicationManager,
    IApplicationRepository repository) : IOpenIddictLegacyImportService
{
    public async Task ImportMissingApplicationsAsync(CancellationToken cancellationToken = default)
    {
        var openIddictApps = new List<object>();
        await foreach (var openIddictApp in applicationManager.ListAsync(cancellationToken: cancellationToken))
        {
            openIddictApps.Add(openIddictApp);
        }

        foreach (var openIddictApp in openIddictApps)
        {
            var clientId = await applicationManager.GetClientIdAsync(openIddictApp, cancellationToken);
            if (string.IsNullOrEmpty(clientId))
            {
                continue;
            }

            if (await repository.GetByClientIdAsync(clientId, cancellationToken) is not null)
            {
                continue;
            }

            var displayName = (await applicationManager.GetDisplayNameAsync(openIddictApp, cancellationToken)) ?? clientId;
            var permissions = (await applicationManager.GetPermissionsAsync(openIddictApp, cancellationToken)).ToList();
            var applicationType = InferType(permissions);
            var now = DateTimeOffset.UtcNow;

            var registered = new RegisteredApplication
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                DisplayName = displayName,
                ApplicationType = applicationType,
                Environment = DeploymentEnvironment.Development,
                Status = ApplicationStatus.Active,
                OpenIddictApplicationId = (await applicationManager.GetIdAsync(openIddictApp, cancellationToken))?.ToString(),
                ConsentType = "explicit",
                CreatedAt = now,
                UpdatedAt = now
            };

            foreach (var uri in await applicationManager.GetRedirectUrisAsync(openIddictApp, cancellationToken))
            {
                registered.RedirectUris.Add(new RegisteredRedirectUri
                {
                    Id = Guid.NewGuid(),
                    Uri = uri,
                    Kind = RedirectUriKind.Redirect
                });
            }

            foreach (var uri in await applicationManager.GetPostLogoutRedirectUrisAsync(openIddictApp, cancellationToken))
            {
                registered.RedirectUris.Add(new RegisteredRedirectUri
                {
                    Id = Guid.NewGuid(),
                    Uri = uri,
                    Kind = RedirectUriKind.PostLogout
                });
            }

            foreach (var scope in ExtractScopes(permissions))
            {
                registered.Scopes.Add(new RegisteredApplicationScope
                {
                    Id = Guid.NewGuid(),
                    Scope = scope
                });
            }

            if (registered.Scopes.Count == 0)
            {
                registered.Scopes.Add(new RegisteredApplicationScope
                {
                    Id = Guid.NewGuid(),
                    Scope = ScopeNames.OpenId
                });
            }

            await repository.AddAsync(registered, cancellationToken);
        }

        await repository.SaveChangesAsync(cancellationToken);
    }

    private static ApplicationType InferType(IReadOnlyCollection<string> permissions)
    {
        if (permissions.Contains(Permissions.GrantTypes.ClientCredentials))
        {
            return ApplicationType.ServiceM2m;
        }

        if (permissions.Contains(Permissions.GrantTypes.AuthorizationCode))
        {
            return permissions.Contains(Permissions.Endpoints.Authorization)
                ? ApplicationType.SpaPublic
                : ApplicationType.WebConfidential;
        }

        return ApplicationType.SpaPublic;
    }

    private static IEnumerable<string> ExtractScopes(IEnumerable<string> permissions) =>
        permissions
            .Where(permission => permission.StartsWith(Permissions.Prefixes.Scope, StringComparison.Ordinal))
            .Select(permission => permission[Permissions.Prefixes.Scope.Length..]);
}
