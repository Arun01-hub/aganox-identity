using Aganox.IdentityServer.Domain.Enums;

namespace Aganox.IdentityServer.Application.Models.Applications;

public sealed class ApplicationAuditDto
{
    public required Guid Id { get; init; }

    public ApplicationAuditAction Action { get; init; }

    public string? ActorUserId { get; init; }

    public DateTimeOffset OccurredAt { get; init; }

    public required string Summary { get; init; }

    public string? DetailsJson { get; init; }
}
