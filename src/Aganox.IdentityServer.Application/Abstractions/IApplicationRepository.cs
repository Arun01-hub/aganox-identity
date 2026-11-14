using Aganox.IdentityServer.Application.Models.Applications;
using Aganox.IdentityServer.Domain.Entities;
using Aganox.IdentityServer.Domain.Enums;

namespace Aganox.IdentityServer.Application.Abstractions;

public interface IApplicationRepository
{
    Task<RegisteredApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<RegisteredApplication?> GetByClientIdAsync(string clientId, CancellationToken cancellationToken = default);

    Task<PagedResult<RegisteredApplication>> ListAsync(ApplicationListQuery query, CancellationToken cancellationToken = default);

    Task AddAsync(RegisteredApplication application, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    Task RemoveChildCollectionsAsync(RegisteredApplication application, CancellationToken cancellationToken = default);

    Task ClearRedirectUrisAsync(RegisteredApplication application, CancellationToken cancellationToken = default);

    Task ClearAllowedOriginsAsync(RegisteredApplication application, CancellationToken cancellationToken = default);

    Task ClearScopesAsync(RegisteredApplication application, CancellationToken cancellationToken = default);

    Task InsertRedirectUrisAsync(
        Guid applicationId,
        IReadOnlyList<string> redirectUris,
        IReadOnlyList<string> postLogoutRedirectUris,
        CancellationToken cancellationToken = default);

    Task InsertAllowedOriginsAsync(
        Guid applicationId,
        IReadOnlyList<string> allowedOrigins,
        CancellationToken cancellationToken = default);

    Task InsertScopesAsync(
        Guid applicationId,
        IReadOnlyList<string> scopes,
        CancellationToken cancellationToken = default);

    Task<RegisteredApplication> ReloadAsync(Guid id, CancellationToken cancellationToken = default);

    Task TouchUpdatedAtAsync(Guid id, DateTimeOffset updatedAt, CancellationToken cancellationToken = default);

    void MarkUnchanged(RegisteredApplication application);

    Task ApplyDevelopmentDefinitionMetadataAsync(
        Guid id,
        string displayName,
        ApplicationType applicationType,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken = default);

    void PrepareForChildDefinitionReplace(RegisteredApplication application);

    Task<IReadOnlyList<string>> GetActiveSpaOriginsAsync(CancellationToken cancellationToken = default);
}
