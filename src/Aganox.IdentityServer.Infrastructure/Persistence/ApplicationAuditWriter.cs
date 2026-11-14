using System.Text.Json;
using Aganox.IdentityServer.Application.Abstractions;
using Aganox.IdentityServer.Domain.Entities;
using Aganox.IdentityServer.Domain.Enums;

namespace Aganox.IdentityServer.Infrastructure.Persistence;

public sealed class ApplicationAuditWriter(AuthDbContext context) : IApplicationAuditWriter
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = false };

    public async Task WriteAsync(
        Guid applicationId,
        ApplicationAuditAction action,
        string summary,
        string? actorUserId,
        object? details = null,
        CancellationToken cancellationToken = default)
    {
        var entry = new ApplicationAuditEntry
        {
            Id = Guid.NewGuid(),
            RegisteredApplicationId = applicationId,
            Action = action,
            ActorUserId = actorUserId,
            OccurredAt = DateTimeOffset.UtcNow,
            Summary = summary,
            DetailsJson = details is null ? null : JsonSerializer.Serialize(details, SerializerOptions)
        };

        await context.ApplicationAuditEntries.AddAsync(entry, cancellationToken);
    }
}
