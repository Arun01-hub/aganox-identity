using Aganox.IdentityServer.Application.Models.Applications;
using Aganox.IdentityServer.Domain.Enums;

namespace Aganox.IdentityServer.Application.Abstractions;

public interface IApplicationRegistrationService
{
    Task<RegisterApplicationResult> RegisterAsync(
        RegisterApplicationRequest request,
        ActorContext actor,
        CancellationToken cancellationToken = default);

    Task<ApplicationDetailDto?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<ApplicationDetailDto>> ListAsync(
        ApplicationListQuery query,
        CancellationToken cancellationToken = default);

    Task<ApplicationDetailDto> UpdateMetadataAsync(
        Guid id,
        string displayName,
        string? description,
        ActorContext actor,
        CancellationToken cancellationToken = default);

    Task<ApplicationDetailDto> UpdateRedirectUrisAsync(
        Guid id,
        IReadOnlyList<string> redirectUris,
        IReadOnlyList<string> postLogoutRedirectUris,
        ActorContext actor,
        CancellationToken cancellationToken = default);

    Task<ApplicationDetailDto> UpdateOriginsAsync(
        Guid id,
        IReadOnlyList<string> allowedOrigins,
        ActorContext actor,
        CancellationToken cancellationToken = default);

    Task<ApplicationDetailDto> UpdateScopesAsync(
        Guid id,
        IReadOnlyList<string> scopes,
        ActorContext actor,
        CancellationToken cancellationToken = default);

    Task<ApplicationDetailDto> ChangeStatusAsync(
        Guid id,
        ApplicationStatus targetStatus,
        ActorContext actor,
        CancellationToken cancellationToken = default);

    Task<RotateSecretResult> RotateSecretAsync(
        Guid id,
        ActorContext actor,
        CancellationToken cancellationToken = default);

    Task<ApplicationDetailDto> RevokeSecretAsync(
        Guid id,
        ActorContext actor,
        CancellationToken cancellationToken = default);

    Task<PagedResult<ApplicationAuditDto>> GetAuditAsync(
        Guid id,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    void InvalidateCorsCache();
}
