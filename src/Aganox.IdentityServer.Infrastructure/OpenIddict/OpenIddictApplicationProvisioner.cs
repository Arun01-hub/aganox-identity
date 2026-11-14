using Aganox.IdentityServer.Application.Abstractions;
using Aganox.IdentityServer.Domain.Entities;
using Aganox.IdentityServer.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Aganox.IdentityServer.Infrastructure.OpenIddict;

public sealed class OpenIddictApplicationProvisioner(
    IOpenIddictApplicationManager applicationManager,
    IOpenIddictTokenManager tokenManager) : IIdentityProviderProvisioner
{
    private const int MaxUpdateAttempts = 3;

    public async Task ProvisionAsync(
        RegisteredApplication application,
        string? plaintextSecret,
        CancellationToken cancellationToken = default)
    {
        var enabled = application.Status == ApplicationStatus.Active;
        if (await applicationManager.FindByClientIdAsync(application.ClientId, cancellationToken) is not null)
        {
            await UpdateExistingWithRetryAsync(application, enabled, plaintextSecret, cancellationToken);
            return;
        }

        await CreateAsync(application, enabled, plaintextSecret, cancellationToken);
    }

    public Task UpdateAsync(
        RegisteredApplication application,
        string? plaintextSecret,
        CancellationToken cancellationToken = default) =>
        ProvisionAsync(application, plaintextSecret, cancellationToken);

    public Task ApplyStatusAsync(RegisteredApplication application, CancellationToken cancellationToken = default)
    {
        var enabled = application.Status == ApplicationStatus.Active;
        return UpdateDescriptorAsync(application, enabled, null, cancellationToken);
    }

    public async Task RevokeTokensAsync(RegisteredApplication application, CancellationToken cancellationToken = default)
    {
        if (await applicationManager.FindByClientIdAsync(application.ClientId, cancellationToken) is not { } openIddictApp)
        {
            return;
        }

        var applicationId = await applicationManager.GetIdAsync(openIddictApp, cancellationToken);
        if (applicationId is null)
        {
            return;
        }

        await foreach (var token in tokenManager.FindByApplicationIdAsync(applicationId.ToString()!, cancellationToken))
        {
            await tokenManager.DeleteAsync(token, cancellationToken);
        }
    }

    private async Task UpdateDescriptorAsync(
        RegisteredApplication application,
        bool enabled,
        string? plaintextSecret,
        CancellationToken cancellationToken)
    {
        if (await applicationManager.FindByClientIdAsync(application.ClientId, cancellationToken) is not { } existing)
        {
            if (enabled)
            {
                await ProvisionAsync(application, plaintextSecret, cancellationToken);
            }

            return;
        }

        await UpdateExistingWithRetryAsync(application, enabled, plaintextSecret, cancellationToken);
    }

    private async Task CreateAsync(
        RegisteredApplication application,
        bool enabled,
        string? plaintextSecret,
        CancellationToken cancellationToken)
    {
        var createDescriptor = await BuildDescriptorAsync(null, application, enabled, plaintextSecret, cancellationToken);
        await applicationManager.CreateAsync(createDescriptor, cancellationToken);
        await AssignOpenIddictApplicationIdAsync(application, cancellationToken);
    }

    private async Task UpdateExistingWithRetryAsync(
        RegisteredApplication application,
        bool enabled,
        string? plaintextSecret,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= MaxUpdateAttempts; attempt++)
        {
            if (await applicationManager.FindByClientIdAsync(application.ClientId, cancellationToken) is not { } existing)
            {
                await CreateAsync(application, enabled, plaintextSecret, cancellationToken);
                return;
            }

            var descriptor = await BuildDescriptorAsync(existing, application, enabled, plaintextSecret, cancellationToken);
            try
            {
                await applicationManager.UpdateAsync(existing, descriptor, cancellationToken);
                application.OpenIddictApplicationId =
                    (await applicationManager.GetIdAsync(existing, cancellationToken))?.ToString();
                return;
            }
            catch (Exception exception) when (IsConcurrencyConflict(exception) && attempt < MaxUpdateAttempts)
            {
            }
        }

        throw new InvalidOperationException(
            $"Failed to update OpenIddict application '{application.ClientId}' after {MaxUpdateAttempts} attempts due to concurrent modifications.");
    }

    private async Task AssignOpenIddictApplicationIdAsync(
        RegisteredApplication application,
        CancellationToken cancellationToken)
    {
        var created = await applicationManager.FindByClientIdAsync(application.ClientId, cancellationToken);
        application.OpenIddictApplicationId = created is null
            ? null
            : (await applicationManager.GetIdAsync(created, cancellationToken))?.ToString();
    }

    private static bool IsConcurrencyConflict(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is OpenIddictExceptions.ConcurrencyException or DbUpdateConcurrencyException)
            {
                return true;
            }
        }

        return false;
    }

    private async Task<OpenIddictApplicationDescriptor> BuildDescriptorAsync(
        object? existing,
        RegisteredApplication application,
        bool enabled,
        string? plaintextSecret,
        CancellationToken cancellationToken)
    {
        var descriptor = new OpenIddictApplicationDescriptor();
        if (existing is not null)
        {
            await applicationManager.PopulateAsync(descriptor, existing, cancellationToken);
        }

        descriptor.ClientId = application.ClientId;
        descriptor.DisplayName = application.DisplayName;
        descriptor.ClientType = application.ApplicationType switch
        {
            ApplicationType.SpaPublic => ClientTypes.Public,
            ApplicationType.WebConfidential => ClientTypes.Confidential,
            ApplicationType.ServiceM2m => ClientTypes.Confidential,
            _ => ClientTypes.Public
        };
        descriptor.ConsentType = ConsentTypes.Explicit;

        descriptor.Permissions.Clear();
        foreach (var permission in OpenIddictPermissionMapper.BuildPermissions(application, enabled))
        {
            descriptor.Permissions.Add(permission);
        }

        descriptor.RedirectUris.Clear();
        foreach (var redirectUri in application.RedirectUris.Where(uri => uri.Kind == RedirectUriKind.Redirect))
        {
            descriptor.RedirectUris.Add(new Uri(redirectUri.Uri));
        }

        descriptor.PostLogoutRedirectUris.Clear();
        foreach (var postLogoutUri in application.RedirectUris.Where(uri => uri.Kind == RedirectUriKind.PostLogout))
        {
            descriptor.PostLogoutRedirectUris.Add(new Uri(postLogoutUri.Uri));
        }

        if (!string.IsNullOrEmpty(plaintextSecret))
        {
            descriptor.ClientSecret = plaintextSecret;
        }

        return descriptor;
    }
}
