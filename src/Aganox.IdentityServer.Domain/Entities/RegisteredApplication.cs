using Aganox.IdentityServer.Domain.Enums;

namespace Aganox.IdentityServer.Domain.Entities;

public class RegisteredApplication
{
    public Guid Id { get; set; }

    public required string ClientId { get; set; }

    public required string DisplayName { get; set; }

    public string? Description { get; set; }

    public ApplicationType ApplicationType { get; set; }

    public DeploymentEnvironment Environment { get; set; }

    public ApplicationStatus Status { get; set; }

    public string? OpenIddictApplicationId { get; set; }

    public string ConsentType { get; set; } = "explicit";

    public string? CreatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<RegisteredRedirectUri> RedirectUris { get; set; } = [];

    public ICollection<RegisteredAllowedOrigin> AllowedOrigins { get; set; } = [];

    public ICollection<RegisteredApplicationScope> Scopes { get; set; } = [];

    public ICollection<ClientCredential> Credentials { get; set; } = [];

    public ICollection<ApplicationAuditEntry> AuditEntries { get; set; } = [];
}
