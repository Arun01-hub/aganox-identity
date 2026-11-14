using Aganox.IdentityServer.Domain.Enums;

namespace Aganox.IdentityServer.Application.Abstractions;

public interface IApplicationAuditWriter
{
    Task WriteAsync(
        Guid applicationId,
        ApplicationAuditAction action,
        string summary,
        string? actorUserId,
        object? details = null,
        CancellationToken cancellationToken = default);
}
