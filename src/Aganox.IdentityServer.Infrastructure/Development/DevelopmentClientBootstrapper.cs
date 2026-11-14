using Aganox.IdentityServer.Application.Abstractions;
using Aganox.IdentityServer.Domain.Entities;
using Aganox.IdentityServer.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Aganox.IdentityServer.Infrastructure.Development;

public sealed class DevelopmentClientBootstrapper(
    IServiceScopeFactory scopeFactory,
    IClientSecretHasher secretHasher) : IDevelopmentClientBootstrapper
{
    private static readonly DevClientDefinition[] Clients =
    [
        new(
            "aganox-web",
            "Aganox Web",
            ApplicationType.SpaPublic,
            null,
            ["https://localhost:7036/signin-oidc", "http://localhost:5110/signin-oidc"],
            [],
            [],
            [ScopeNames.OpenId, ScopeNames.Profile, ScopeNames.Email, ScopeNames.OfflineAccess, ScopeNames.AganoxApi]),
        new(
            "aganox-service",
            "Aganox Service",
            ApplicationType.ServiceM2m,
            "dev-service-secret-change-in-production",
            [],
            [],
            [],
            [ScopeNames.AganoxApi]),
        new(
            "aganox-rental",
            "Aganox Rental",
            ApplicationType.SpaPublic,
            null,
            [
                "http://localhost:4200/",
                "http://localhost:4200",
                "http://rental.aganox.local:4200/",
                "http://rental.aganox.local:4200"
            ],
            [
                "http://localhost:4200/",
                "http://localhost:4200",
                "http://rental.aganox.local:4200/",
                "http://rental.aganox.local:4200"
            ],
            ["http://localhost:4200", "http://rental.aganox.local:4200"],
            [ScopeNames.OpenId, ScopeNames.Profile, ScopeNames.Email, ScopeNames.OfflineAccess, ScopeNames.AganoxApi]),
        new(
            "aganox-rental-service",
            "Aganox Rental Service",
            ApplicationType.ServiceM2m,
            "dev-rental-service-secret-change-in-production",
            [],
            [],
            [],
            [ScopeNames.AganoxApi])
    ];

    public async Task EnsureDevelopmentClientsAsync(CancellationToken cancellationToken = default)
    {
        foreach (var definition in Clients)
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<IApplicationRepository>();
            var provisioner = scope.ServiceProvider.GetRequiredService<IIdentityProviderProvisioner>();
            await UpsertClientAsync(definition, repository, provisioner, cancellationToken);
        }
    }

    private async Task UpsertClientAsync(
        DevClientDefinition definition,
        IApplicationRepository repository,
        IIdentityProviderProvisioner provisioner,
        CancellationToken cancellationToken)
    {
        var application = await repository.GetByClientIdAsync(definition.ClientId, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var registryChanged = false;

        if (application is null)
        {
            application = new RegisteredApplication
            {
                Id = Guid.NewGuid(),
                ClientId = definition.ClientId,
                DisplayName = definition.DisplayName,
                ApplicationType = definition.Type,
                Environment = DeploymentEnvironment.Development,
                Status = ApplicationStatus.Active,
                ConsentType = "explicit",
                CreatedAt = now,
                UpdatedAt = now
            };

            ApplyDefinition(application, definition);
            if (!string.IsNullOrEmpty(definition.DevSecret))
            {
                application.Credentials.Add(new ClientCredential
                {
                    Id = Guid.NewGuid(),
                    SecretHash = secretHasher.Hash(definition.DevSecret),
                    SecretPrefix = definition.DevSecret[^4..],
                    IsPrimary = true,
                    CreatedAt = now
                });
            }

            await repository.AddAsync(application, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);
            registryChanged = true;
        }
        else if (!DefinitionMatches(application, definition))
        {
            var applicationId = application.Id;
            await repository.RemoveChildCollectionsAsync(application, cancellationToken);
            await repository.InsertRedirectUrisAsync(
                applicationId,
                definition.RedirectUris,
                definition.PostLogoutRedirectUris,
                cancellationToken);
            await repository.InsertAllowedOriginsAsync(applicationId, definition.AllowedOrigins, cancellationToken);
            await repository.InsertScopesAsync(applicationId, definition.Scopes, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);
            await repository.ApplyDevelopmentDefinitionMetadataAsync(
                applicationId,
                definition.DisplayName,
                definition.Type,
                now,
                cancellationToken);
            application = (await repository.GetByClientIdAsync(definition.ClientId, cancellationToken))!;
            registryChanged = true;
        }

        var provisionSecret = registryChanged ? definition.DevSecret : null;
        await provisioner.ProvisionAsync(application, provisionSecret, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
    }

    private static bool DefinitionMatches(RegisteredApplication application, DevClientDefinition definition)
    {
        if (application.DisplayName != definition.DisplayName ||
            application.ApplicationType != definition.Type ||
            application.Environment != DeploymentEnvironment.Development ||
            application.Status != ApplicationStatus.Active)
        {
            return false;
        }

        if (!SetEquals(
                application.RedirectUris.Where(uri => uri.Kind == RedirectUriKind.Redirect).Select(uri => uri.Uri),
                definition.RedirectUris))
        {
            return false;
        }

        if (!SetEquals(
                application.RedirectUris.Where(uri => uri.Kind == RedirectUriKind.PostLogout).Select(uri => uri.Uri),
                definition.PostLogoutRedirectUris))
        {
            return false;
        }

        if (!SetEquals(application.AllowedOrigins.Select(origin => origin.Origin), definition.AllowedOrigins))
        {
            return false;
        }

        return SetEquals(application.Scopes.Select(scope => scope.Scope), definition.Scopes);
    }

    private static bool SetEquals(IEnumerable<string> actual, IReadOnlyList<string> expected)
    {
        var actualSet = actual.ToHashSet(StringComparer.Ordinal);
        var expectedSet = expected.ToHashSet(StringComparer.Ordinal);
        return actualSet.SetEquals(expectedSet);
    }

    private static void ApplyDefinition(RegisteredApplication application, DevClientDefinition definition)
    {
        foreach (var uri in definition.RedirectUris)
        {
            application.RedirectUris.Add(new RegisteredRedirectUri
            {
                Id = Guid.NewGuid(),
                RegisteredApplicationId = application.Id,
                Uri = uri,
                Kind = RedirectUriKind.Redirect
            });
        }

        foreach (var uri in definition.PostLogoutRedirectUris)
        {
            application.RedirectUris.Add(new RegisteredRedirectUri
            {
                Id = Guid.NewGuid(),
                RegisteredApplicationId = application.Id,
                Uri = uri,
                Kind = RedirectUriKind.PostLogout
            });
        }

        foreach (var origin in definition.AllowedOrigins)
        {
            application.AllowedOrigins.Add(new RegisteredAllowedOrigin
            {
                Id = Guid.NewGuid(),
                RegisteredApplicationId = application.Id,
                Origin = origin
            });
        }

        foreach (var scope in definition.Scopes)
        {
            application.Scopes.Add(new RegisteredApplicationScope
            {
                Id = Guid.NewGuid(),
                RegisteredApplicationId = application.Id,
                Scope = scope
            });
        }
    }

    private sealed record DevClientDefinition(
        string ClientId,
        string DisplayName,
        ApplicationType Type,
        string? DevSecret,
        IReadOnlyList<string> RedirectUris,
        IReadOnlyList<string> PostLogoutRedirectUris,
        IReadOnlyList<string> AllowedOrigins,
        IReadOnlyList<string> Scopes);
}
