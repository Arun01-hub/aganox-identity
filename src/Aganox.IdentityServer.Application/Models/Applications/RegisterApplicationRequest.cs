using Aganox.IdentityServer.Domain.Enums;

namespace Aganox.IdentityServer.Application.Models.Applications;

public sealed class RegisterApplicationRequest
{
    public required string DisplayName { get; init; }

    public string? Description { get; init; }

    public ApplicationType ApplicationType { get; init; }

    public DeploymentEnvironment Environment { get; init; }

    public string? Slug { get; init; }

    public string? ClientId { get; init; }

    public IReadOnlyList<string> RedirectUris { get; init; } = [];

    public IReadOnlyList<string> PostLogoutRedirectUris { get; init; } = [];

    public IReadOnlyList<string> AllowedOrigins { get; init; } = [];

    public IReadOnlyList<string> Scopes { get; init; } = [];

    public bool ActivateImmediately { get; init; }
}
