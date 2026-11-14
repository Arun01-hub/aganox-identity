namespace Aganox.IdentityServer.Application.Models.Applications;

public sealed class ClientCredentialSummaryDto
{
    public required Guid Id { get; init; }

    public string? SecretPrefix { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? RevokedAt { get; init; }

    public bool IsPrimary { get; init; }
}
