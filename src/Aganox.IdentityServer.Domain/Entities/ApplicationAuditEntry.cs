using Aganox.IdentityServer.Domain.Enums;

namespace Aganox.IdentityServer.Domain.Entities;

public class ApplicationAuditEntry
{
    public Guid Id { get; set; }

    public Guid RegisteredApplicationId { get; set; }

    public RegisteredApplication RegisteredApplication { get; set; } = null!;

    public ApplicationAuditAction Action { get; set; }

    public string? ActorUserId { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    public required string Summary { get; set; }

    public string? DetailsJson { get; set; }
}
