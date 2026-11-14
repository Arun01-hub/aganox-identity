namespace Aganox.IdentityServer.Domain.Entities;

public class ClientCredential
{
    public Guid Id { get; set; }

    public Guid RegisteredApplicationId { get; set; }

    public RegisteredApplication RegisteredApplication { get; set; } = null!;

    public required string SecretHash { get; set; }

    public string? SecretPrefix { get; set; }

    public bool IsPrimary { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public string? CreatedByUserId { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }

    public string? RevokedByUserId { get; set; }
}
