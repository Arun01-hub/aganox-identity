using Aganox.IdentityServer.Application.Abstractions;
using Aganox.IdentityServer.Application.Models.Applications;
using Aganox.IdentityServer.Domain.Entities;
using Aganox.IdentityServer.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Aganox.IdentityServer.Infrastructure.Persistence;

public sealed class ApplicationRepository(AuthDbContext context) : IApplicationRepository
{
    public Task<RegisteredApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.RegisteredApplications
            .AsSplitQuery()
            .Include(application => application.RedirectUris)
            .Include(application => application.AllowedOrigins)
            .Include(application => application.Scopes)
            .Include(application => application.Credentials)
            .Include(application => application.AuditEntries)
            .FirstOrDefaultAsync(application => application.Id == id, cancellationToken);

    public Task<RegisteredApplication?> GetByClientIdAsync(string clientId, CancellationToken cancellationToken = default) =>
        context.RegisteredApplications
            .AsSplitQuery()
            .Include(application => application.RedirectUris)
            .Include(application => application.AllowedOrigins)
            .Include(application => application.Scopes)
            .Include(application => application.Credentials)
            .FirstOrDefaultAsync(application => application.ClientId == clientId, cancellationToken);

    public async Task<PagedResult<RegisteredApplication>> ListAsync(
        ApplicationListQuery query,
        CancellationToken cancellationToken = default)
    {
        var applications = context.RegisteredApplications
            .AsSplitQuery()
            .Include(application => application.RedirectUris)
            .Include(application => application.AllowedOrigins)
            .Include(application => application.Scopes)
            .Include(application => application.Credentials)
            .AsQueryable();

        if (!query.IncludeArchived)
        {
            applications = applications.Where(application => application.Status != ApplicationStatus.Archived);
        }

        if (query.Status is { } status)
        {
            applications = applications.Where(application => application.Status == status);
        }

        if (query.Environment is { } environment)
        {
            applications = applications.Where(application => application.Environment == environment);
        }

        if (query.ApplicationType is { } type)
        {
            applications = applications.Where(application => application.ApplicationType == type);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            applications = applications.Where(application =>
                application.DisplayName.Contains(search) ||
                application.ClientId.Contains(search));
        }

        var total = await applications.CountAsync(cancellationToken);
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var items = await applications
            .OrderBy(application => application.DisplayName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<RegisteredApplication>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task AddAsync(RegisteredApplication application, CancellationToken cancellationToken = default) =>
        await context.RegisteredApplications.AddAsync(application, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in context.ChangeTracker.Entries()
                     .Where(entry => entry.State == EntityState.Deleted)
                     .ToList())
        {
            entry.State = EntityState.Detached;
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveChildCollectionsAsync(
        RegisteredApplication application,
        CancellationToken cancellationToken = default)
    {
        await ClearRedirectUrisAsync(application, cancellationToken);
        await ClearAllowedOriginsAsync(application, cancellationToken);
        await ClearScopesAsync(application, cancellationToken);
    }

    public async Task ClearRedirectUrisAsync(
        RegisteredApplication application,
        CancellationToken cancellationToken = default)
    {
        var applicationId = application.Id;
        DetachRedirectUris(applicationId);
        application.RedirectUris.Clear();
        await context.RegisteredRedirectUris
            .Where(uri => uri.RegisteredApplicationId == applicationId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task ClearAllowedOriginsAsync(
        RegisteredApplication application,
        CancellationToken cancellationToken = default)
    {
        var applicationId = application.Id;
        DetachAllowedOrigins(applicationId);
        application.AllowedOrigins.Clear();
        await context.RegisteredAllowedOrigins
            .Where(origin => origin.RegisteredApplicationId == applicationId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task ClearScopesAsync(
        RegisteredApplication application,
        CancellationToken cancellationToken = default)
    {
        var applicationId = application.Id;
        DetachScopes(applicationId);
        application.Scopes.Clear();
        await context.RegisteredApplicationScopes
            .Where(scope => scope.RegisteredApplicationId == applicationId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task InsertRedirectUrisAsync(
        Guid applicationId,
        IReadOnlyList<string> redirectUris,
        IReadOnlyList<string> postLogoutRedirectUris,
        CancellationToken cancellationToken = default)
    {
        foreach (var uri in redirectUris.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            await context.RegisteredRedirectUris.AddAsync(
                new RegisteredRedirectUri
                {
                    Id = Guid.NewGuid(),
                    RegisteredApplicationId = applicationId,
                    Uri = uri,
                    Kind = RedirectUriKind.Redirect
                },
                cancellationToken);
        }

        foreach (var uri in postLogoutRedirectUris.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            await context.RegisteredRedirectUris.AddAsync(
                new RegisteredRedirectUri
                {
                    Id = Guid.NewGuid(),
                    RegisteredApplicationId = applicationId,
                    Uri = uri,
                    Kind = RedirectUriKind.PostLogout
                },
                cancellationToken);
        }
    }

    public async Task InsertAllowedOriginsAsync(
        Guid applicationId,
        IReadOnlyList<string> allowedOrigins,
        CancellationToken cancellationToken = default)
    {
        foreach (var origin in allowedOrigins.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            await context.RegisteredAllowedOrigins.AddAsync(
                new RegisteredAllowedOrigin
                {
                    Id = Guid.NewGuid(),
                    RegisteredApplicationId = applicationId,
                    Origin = origin
                },
                cancellationToken);
        }
    }

    public async Task InsertScopesAsync(
        Guid applicationId,
        IReadOnlyList<string> scopes,
        CancellationToken cancellationToken = default)
    {
        foreach (var scope in scopes.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            await context.RegisteredApplicationScopes.AddAsync(
                new RegisteredApplicationScope
                {
                    Id = Guid.NewGuid(),
                    RegisteredApplicationId = applicationId,
                    Scope = scope
                },
                cancellationToken);
        }
    }

    public async Task<RegisteredApplication> ReloadAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (await context.RegisteredApplications.FindAsync([id], cancellationToken) is { } tracked)
        {
            DetachTrackedChildren(id);
            context.Entry(tracked).State = EntityState.Detached;
        }

        return (await GetByIdAsync(id, cancellationToken))!;
    }

    public Task TouchUpdatedAtAsync(
        Guid id,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken = default) =>
        context.RegisteredApplications
            .Where(application => application.Id == id)
            .ExecuteUpdateAsync(
                calls => calls.SetProperty(application => application.UpdatedAt, updatedAt),
                cancellationToken);

    public void MarkUnchanged(RegisteredApplication application) =>
        context.Entry(application).State = EntityState.Unchanged;

    public Task ApplyDevelopmentDefinitionMetadataAsync(
        Guid id,
        string displayName,
        ApplicationType applicationType,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken = default) =>
        context.RegisteredApplications
            .Where(application => application.Id == id)
            .ExecuteUpdateAsync(
                calls => calls
                    .SetProperty(application => application.DisplayName, displayName)
                    .SetProperty(application => application.ApplicationType, applicationType)
                    .SetProperty(application => application.Environment, DeploymentEnvironment.Development)
                    .SetProperty(application => application.Status, ApplicationStatus.Active)
                    .SetProperty(application => application.UpdatedAt, updatedAt),
                cancellationToken);

    public void PrepareForChildDefinitionReplace(RegisteredApplication application) =>
        DetachTrackedChildren(application.Id);

    private void DetachTrackedChildren(Guid applicationId)
    {
        DetachRedirectUris(applicationId);
        DetachAllowedOrigins(applicationId);
        DetachScopes(applicationId);
    }

    private void DetachRedirectUris(Guid applicationId)
    {
        foreach (var entry in context.ChangeTracker.Entries<RegisteredRedirectUri>()
                     .Where(entry => entry.Entity.RegisteredApplicationId == applicationId)
                     .ToList())
        {
            entry.State = EntityState.Detached;
        }
    }

    private void DetachAllowedOrigins(Guid applicationId)
    {
        foreach (var entry in context.ChangeTracker.Entries<RegisteredAllowedOrigin>()
                     .Where(entry => entry.Entity.RegisteredApplicationId == applicationId)
                     .ToList())
        {
            entry.State = EntityState.Detached;
        }
    }

    private void DetachScopes(Guid applicationId)
    {
        foreach (var entry in context.ChangeTracker.Entries<RegisteredApplicationScope>()
                     .Where(entry => entry.Entity.RegisteredApplicationId == applicationId)
                     .ToList())
        {
            entry.State = EntityState.Detached;
        }
    }

    public async Task<IReadOnlyList<string>> GetActiveSpaOriginsAsync(CancellationToken cancellationToken = default) =>
        await context.RegisteredApplications
            .Where(application =>
                application.Status == ApplicationStatus.Active &&
                application.ApplicationType == ApplicationType.SpaPublic)
            .SelectMany(application => application.AllowedOrigins)
            .Select(origin => origin.Origin)
            .Distinct()
            .ToListAsync(cancellationToken);
}
