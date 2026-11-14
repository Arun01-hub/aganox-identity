namespace Aganox.IdentityServer.Domain.Enums;

public enum ApplicationAuditAction
{
    Create = 0,
    UpdateMetadata = 1,
    RedirectUriChange = 2,
    ScopeChange = 3,
    StatusChange = 4,
    SecretGenerated = 5,
    SecretRotated = 6,
    SecretRevoked = 7,
    Provisioned = 8,
    Deprovisioned = 9,
    OriginChange = 10
}
