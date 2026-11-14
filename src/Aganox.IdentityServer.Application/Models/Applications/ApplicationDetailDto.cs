using Aganox.IdentityServer.Domain.Enums;

namespace Aganox.IdentityServer.Application.Models.Applications;

public sealed class ApplicationDetailDto
{
    public required Guid Id { get; init; }

    public required string ClientId { get; init; }

    public required string DisplayName { get; init; }

    public string? Description { get; init; }

    public ApplicationType ApplicationType { get; init; }

    public DeploymentEnvironment Environment { get; init; }

    public ApplicationStatus Status { get; init; }

    public IReadOnlyList<string> RedirectUris { get; init; } = [];

    public IReadOnlyList<string> PostLogoutRedirectUris { get; init; } = [];

    public IReadOnlyList<string> AllowedOrigins { get; init; } = [];

    public IReadOnlyList<string> Scopes { get; init; } = [];

    public ClientCredentialSummaryDto? PrimaryCredential { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }
}
