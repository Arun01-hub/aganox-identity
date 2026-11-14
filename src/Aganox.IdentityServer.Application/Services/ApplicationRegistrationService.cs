using Aganox.IdentityServer.Application.Abstractions;
using Aganox.IdentityServer.Application.Models.Applications;
using Aganox.IdentityServer.Application.Options;
using Aganox.IdentityServer.Application.Validation;
using Aganox.IdentityServer.Domain.Entities;
using Aganox.IdentityServer.Domain.Enums;
using Microsoft.Extensions.Options;

namespace Aganox.IdentityServer.Application.Services;

public sealed class ApplicationRegistrationService(
    IApplicationRepository repository,
    IApplicationAuditWriter auditWriter,
    IIdentityProviderProvisioner provisioner,
    IClientSecretGenerator secretGenerator,
    IClientSecretHasher secretHasher,
    ApplicationRegistrationValidator registrationValidator,
    RedirectUriValidator redirectUriValidator,
    ClientIdGenerator clientIdGenerator,
    ICorsOriginCache corsOriginCache,
    IOptions<ApplicationRegistryOptions> registryOptions) : IApplicationRegistrationService
{
    public async Task<RegisterApplicationResult> RegisterAsync(
        RegisterApplicationRequest request,
        ActorContext actor,
        CancellationToken cancellationToken = default)
    {
        registrationValidator.ValidateRegister(request);

        var clientId = ResolveClientId(request);
        if (await repository.GetByClientIdAsync(clientId, cancellationToken) is not null)
        {
            throw new ApplicationValidationException($"Client ID '{clientId}' is already registered.");
        }

        var now = DateTimeOffset.UtcNow;
        var application = new RegisteredApplication
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            DisplayName = request.DisplayName.Trim(),
            Description = request.Description?.Trim(),
            ApplicationType = request.ApplicationType,
            Environment = request.Environment,
            Status = ApplicationStatus.Pending,
            ConsentType = "explicit",
            CreatedByUserId = actor.UserId,
            CreatedAt = now,
            UpdatedAt = now
        };

        ApplyRedirectUris(application, request.RedirectUris, request.PostLogoutRedirectUris);
        ApplyOrigins(application, request.AllowedOrigins);
        ApplyScopes(application, request.Scopes);

        string? plaintextSecret = null;
        if (RequiresSecret(request.ApplicationType))
        {
            plaintextSecret = secretGenerator.Generate();
            AddCredential(application, plaintextSecret, actor.UserId, now);
        }

        await repository.AddAsync(application, cancellationToken);
        await auditWriter.WriteAsync(
            application.Id,
            ApplicationAuditAction.Create,
            $"Registered application '{application.DisplayName}'.",
            actor.UserId,
            new { application.ClientId, application.ApplicationType, application.Environment },
            cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        if (request.ActivateImmediately)
        {
            application = (await repository.GetByIdAsync(application.Id, cancellationToken))!;
            await ActivateInternalAsync(application, plaintextSecret, actor, cancellationToken);
            application = (await repository.GetByIdAsync(application.Id, cancellationToken))!;
        }

        InvalidateCorsCache();
        return new RegisterApplicationResult
        {
            Application = ApplicationMapper.ToDetailDto(application),
            PlaintextSecret = plaintextSecret
        };
    }

    public async Task<ApplicationDetailDto?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var application = await repository.GetByIdAsync(id, cancellationToken);
        return application is null ? null : ApplicationMapper.ToDetailDto(application);
    }

    public async Task<PagedResult<ApplicationDetailDto>> ListAsync(
        ApplicationListQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = await repository.ListAsync(query, cancellationToken);
        return new PagedResult<ApplicationDetailDto>
        {
            Items = page.Items.Select(ApplicationMapper.ToDetailDto).ToList(),
            TotalCount = page.TotalCount,
            Page = page.Page,
            PageSize = page.PageSize
        };
    }

    public async Task<ApplicationDetailDto> UpdateMetadataAsync(
        Guid id,
        string displayName,
        string? description,
        ActorContext actor,
        CancellationToken cancellationToken = default)
    {
        var application = await GetRequiredAsync(id, cancellationToken);
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ApplicationValidationException("Display name is required.");
        }

        application.DisplayName = displayName.Trim();
        application.Description = description?.Trim();
        application.UpdatedAt = DateTimeOffset.UtcNow;

        await auditWriter.WriteAsync(
            application.Id,
            ApplicationAuditAction.UpdateMetadata,
            "Updated application metadata.",
            actor.UserId,
            cancellationToken: cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        if (application.Status == ApplicationStatus.Active)
        {
            await provisioner.UpdateAsync(application, null, cancellationToken);
        }

        return ApplicationMapper.ToDetailDto(application);
    }

    public async Task<ApplicationDetailDto> UpdateRedirectUrisAsync(
        Guid id,
        IReadOnlyList<string> redirectUris,
        IReadOnlyList<string> postLogoutRedirectUris,
        ActorContext actor,
        CancellationToken cancellationToken = default)
    {
        var application = await GetRequiredAsync(id, cancellationToken);
        registrationValidator.ValidateTypeRules(
            application.ApplicationType,
            redirectUris,
            postLogoutRedirectUris,
            application.AllowedOrigins.Select(origin => origin.Origin).ToList());

        redirectUriValidator.ValidateUris(application.Environment, redirectUris, "Redirect URI");
        redirectUriValidator.ValidateUris(application.Environment, postLogoutRedirectUris, "Post-logout redirect URI");

        var before = SerializeUris(application);
        await repository.ClearRedirectUrisAsync(application, cancellationToken);
        await repository.InsertRedirectUrisAsync(
            id,
            redirectUris,
            postLogoutRedirectUris,
            cancellationToken);

        await auditWriter.WriteAsync(
            id,
            ApplicationAuditAction.RedirectUriChange,
            "Updated redirect URIs.",
            actor.UserId,
            new
            {
                before,
                after = new
                {
                    redirect = redirectUris,
                    postLogout = postLogoutRedirectUris
                }
            },
            cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        await repository.TouchUpdatedAtAsync(id, DateTimeOffset.UtcNow, cancellationToken);
        application = await GetRequiredAsync(id, cancellationToken);

        if (application.Status == ApplicationStatus.Active)
        {
            await provisioner.UpdateAsync(application, null, cancellationToken);
        }

        return ApplicationMapper.ToDetailDto(application);
    }

    public async Task<ApplicationDetailDto> UpdateOriginsAsync(
        Guid id,
        IReadOnlyList<string> allowedOrigins,
        ActorContext actor,
        CancellationToken cancellationToken = default)
    {
        var application = await GetRequiredAsync(id, cancellationToken);
        if (application.ApplicationType == ApplicationType.ServiceM2m)
        {
            throw new ApplicationValidationException("M2M applications cannot have CORS origins.");
        }

        redirectUriValidator.ValidateOrigins(allowedOrigins);
        var before = application.AllowedOrigins.Select(origin => origin.Origin).ToList();
        await repository.ClearAllowedOriginsAsync(application, cancellationToken);
        await repository.InsertAllowedOriginsAsync(id, allowedOrigins, cancellationToken);

        await auditWriter.WriteAsync(
            application.Id,
            ApplicationAuditAction.OriginChange,
            "Updated allowed origins.",
            actor.UserId,
            new { before, after = allowedOrigins },
            cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        await repository.TouchUpdatedAtAsync(id, DateTimeOffset.UtcNow, cancellationToken);
        InvalidateCorsCache();

        application = await GetRequiredAsync(id, cancellationToken);
        return ApplicationMapper.ToDetailDto(application);
    }

    public async Task<ApplicationDetailDto> UpdateScopesAsync(
        Guid id,
        IReadOnlyList<string> scopes,
        ActorContext actor,
        CancellationToken cancellationToken = default)
    {
        var application = await GetRequiredAsync(id, cancellationToken);
        registrationValidator.ValidateScopes(application.ApplicationType, scopes);

        var before = application.Scopes.Select(scope => scope.Scope).ToList();
        await repository.ClearScopesAsync(application, cancellationToken);
        await repository.InsertScopesAsync(id, scopes, cancellationToken);

        await auditWriter.WriteAsync(
            application.Id,
            ApplicationAuditAction.ScopeChange,
            "Updated allowed scopes.",
            actor.UserId,
            new { before, after = scopes },
            cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        await repository.TouchUpdatedAtAsync(id, DateTimeOffset.UtcNow, cancellationToken);
        application = await GetRequiredAsync(id, cancellationToken);

        if (application.Status == ApplicationStatus.Active)
        {
            await provisioner.UpdateAsync(application, null, cancellationToken);
        }

        return ApplicationMapper.ToDetailDto(application);
    }

    public async Task<ApplicationDetailDto> ChangeStatusAsync(
        Guid id,
        ApplicationStatus targetStatus,
        ActorContext actor,
        CancellationToken cancellationToken = default)
    {
        var application = await GetRequiredAsync(id, cancellationToken);
        var previous = application.Status;
        ApplicationStatusTransitionValidator.Validate(previous, targetStatus);

        application.Status = targetStatus;
        application.UpdatedAt = DateTimeOffset.UtcNow;

        string? activationSecret = null;
        if (targetStatus == ApplicationStatus.Active && previous == ApplicationStatus.Pending &&
            RequiresSecret(application.ApplicationType) &&
            !application.Credentials.Any(credential => credential.IsPrimary && credential.RevokedAt is null))
        {
            activationSecret = secretGenerator.Generate();
            AddCredential(application, activationSecret, actor.UserId, DateTimeOffset.UtcNow);
        }

        await auditWriter.WriteAsync(
            application.Id,
            ApplicationAuditAction.StatusChange,
            $"Status changed from {previous} to {targetStatus}.",
            actor.UserId,
            new { previous, target = targetStatus },
            cancellationToken);

        await repository.SaveChangesAsync(cancellationToken);

        if (targetStatus == ApplicationStatus.Active && previous == ApplicationStatus.Pending)
        {
            await provisioner.ProvisionAsync(application, activationSecret, cancellationToken);
            await auditWriter.WriteAsync(
                application.Id,
                ApplicationAuditAction.Provisioned,
                "Provisioned OpenIddict application.",
                actor.UserId,
                cancellationToken: cancellationToken);
        }
        else
        {
            await provisioner.ApplyStatusAsync(application, cancellationToken);
            if (targetStatus is ApplicationStatus.Revoked or ApplicationStatus.Archived)
            {
                await provisioner.RevokeTokensAsync(application, cancellationToken);
            }
        }

        await repository.SaveChangesAsync(cancellationToken);
        InvalidateCorsCache();

        return ApplicationMapper.ToDetailDto(application);
    }

    public async Task<RotateSecretResult> RotateSecretAsync(
        Guid id,
        ActorContext actor,
        CancellationToken cancellationToken = default)
    {
        var application = await GetRequiredAsync(id, cancellationToken);
        if (!RequiresSecret(application.ApplicationType))
        {
            throw new ApplicationValidationException("This application type does not use client secrets.");
        }

        var now = DateTimeOffset.UtcNow;
        foreach (var credential in application.Credentials.Where(c => c.IsPrimary && c.RevokedAt is null))
        {
            credential.IsPrimary = false;
            credential.RevokedAt = now;
            credential.RevokedByUserId = actor.UserId;
        }

        var plaintext = secretGenerator.Generate();
        AddCredential(application, plaintext, actor.UserId, now);
        application.UpdatedAt = now;

        await auditWriter.WriteAsync(
            application.Id,
            ApplicationAuditAction.SecretRotated,
            "Rotated client secret.",
            actor.UserId,
            cancellationToken: cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        if (application.Status == ApplicationStatus.Active)
        {
            await provisioner.UpdateAsync(application, plaintext, cancellationToken);
        }

        return new RotateSecretResult
        {
            Application = ApplicationMapper.ToDetailDto(application),
            PlaintextSecret = plaintext
        };
    }

    public async Task<ApplicationDetailDto> RevokeSecretAsync(
        Guid id,
        ActorContext actor,
        CancellationToken cancellationToken = default)
    {
        var application = await GetRequiredAsync(id, cancellationToken);
        if (!RequiresSecret(application.ApplicationType))
        {
            throw new ApplicationValidationException("This application type does not use client secrets.");
        }

        var now = DateTimeOffset.UtcNow;
        foreach (var credential in application.Credentials.Where(c => c.IsPrimary && c.RevokedAt is null))
        {
            credential.IsPrimary = false;
            credential.RevokedAt = now;
            credential.RevokedByUserId = actor.UserId;
        }

        application.UpdatedAt = now;
        await auditWriter.WriteAsync(
            application.Id,
            ApplicationAuditAction.SecretRevoked,
            "Revoked client secret.",
            actor.UserId,
            cancellationToken: cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        if (application.Status == ApplicationStatus.Active)
        {
            await provisioner.UpdateAsync(application, null, cancellationToken);
        }

        return ApplicationMapper.ToDetailDto(application);
    }

    public async Task<PagedResult<ApplicationAuditDto>> GetAuditAsync(
        Guid id,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        _ = await GetRequiredAsync(id, cancellationToken);
        var application = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new ApplicationValidationException("Application not found.");

        var entries = application.AuditEntries
            .OrderByDescending(entry => entry.OccurredAt)
            .ToList();

        var total = entries.Count;
        var items = entries
            .Skip(Math.Max(0, page - 1) * pageSize)
            .Take(pageSize)
            .Select(ApplicationMapper.ToAuditDto)
            .ToList();

        return new PagedResult<ApplicationAuditDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public void InvalidateCorsCache() => corsOriginCache.Invalidate();

    private async Task ActivateInternalAsync(
        RegisteredApplication application,
        string? plaintextSecret,
        ActorContext actor,
        CancellationToken cancellationToken)
    {
        ApplicationStatusTransitionValidator.Validate(application.Status, ApplicationStatus.Active);
        application.Status = ApplicationStatus.Active;
        application.UpdatedAt = DateTimeOffset.UtcNow;

        var secret = plaintextSecret;
        if (secret is null &&
            RequiresSecret(application.ApplicationType) &&
            !application.Credentials.Any(credential => credential.IsPrimary && credential.RevokedAt is null))
        {
            secret = secretGenerator.Generate();
            AddCredential(application, secret, actor.UserId, DateTimeOffset.UtcNow);
        }

        await auditWriter.WriteAsync(
            application.Id,
            ApplicationAuditAction.StatusChange,
            "Activated application.",
            actor.UserId,
            new { previous = ApplicationStatus.Pending, target = ApplicationStatus.Active },
            cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        await provisioner.ProvisionAsync(application, secret, cancellationToken);

        await auditWriter.WriteAsync(
            application.Id,
            ApplicationAuditAction.Provisioned,
            "Provisioned OpenIddict application.",
            actor.UserId,
            cancellationToken: cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
    }

    private string ResolveClientId(RegisterApplicationRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.ClientId))
        {
            if (request.Environment == DeploymentEnvironment.Production && !registryOptions.Value.AllowExplicitClientId)
            {
                throw new ApplicationValidationException(
                    "Explicit client IDs are not allowed in Production.");
            }

            return request.ClientId.Trim();
        }

        return clientIdGenerator.Generate(request.DisplayName, request.Environment, request.Slug);
    }

    private async Task<RegisteredApplication> GetRequiredAsync(Guid id, CancellationToken cancellationToken)
    {
        var application = await repository.GetByIdAsync(id, cancellationToken);
        if (application is null)
        {
            throw new ApplicationValidationException("Application not found.");
        }

        return application;
    }

    private static bool RequiresSecret(ApplicationType type) =>
        type is ApplicationType.WebConfidential or ApplicationType.ServiceM2m;

    private void AddCredential(RegisteredApplication application, string plaintext, string? userId, DateTimeOffset now)
    {
        application.Credentials.Add(new ClientCredential
        {
            Id = Guid.NewGuid(),
            SecretHash = secretHasher.Hash(plaintext),
            SecretPrefix = plaintext.Length >= 4 ? plaintext[^4..] : plaintext,
            IsPrimary = true,
            CreatedAt = now,
            CreatedByUserId = userId
        });
    }

    private static void ApplyRedirectUris(
        RegisteredApplication application,
        IReadOnlyList<string> redirectUris,
        IReadOnlyList<string> postLogoutRedirectUris)
    {
        foreach (var uri in redirectUris.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            application.RedirectUris.Add(new RegisteredRedirectUri
            {
                Id = Guid.NewGuid(),
                Uri = uri,
                Kind = RedirectUriKind.Redirect
            });
        }

        foreach (var uri in postLogoutRedirectUris.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            application.RedirectUris.Add(new RegisteredRedirectUri
            {
                Id = Guid.NewGuid(),
                Uri = uri,
                Kind = RedirectUriKind.PostLogout
            });
        }
    }

    private static void ApplyOrigins(RegisteredApplication application, IReadOnlyList<string> allowedOrigins)
    {
        foreach (var origin in allowedOrigins.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            application.AllowedOrigins.Add(new RegisteredAllowedOrigin
            {
                Id = Guid.NewGuid(),
                Origin = origin
            });
        }
    }

    private static void ApplyScopes(RegisteredApplication application, IReadOnlyList<string> scopes)
    {
        foreach (var scope in scopes.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            application.Scopes.Add(new RegisteredApplicationScope
            {
                Id = Guid.NewGuid(),
                Scope = scope
            });
        }
    }

    private static object SerializeUris(RegisteredApplication application) => new
    {
        redirect = application.RedirectUris.Where(u => u.Kind == RedirectUriKind.Redirect).Select(u => u.Uri),
        postLogout = application.RedirectUris.Where(u => u.Kind == RedirectUriKind.PostLogout).Select(u => u.Uri)
    };
}
