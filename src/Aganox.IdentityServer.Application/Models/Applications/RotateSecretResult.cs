namespace Aganox.IdentityServer.Application.Models.Applications;

public sealed class RotateSecretResult
{
    public required ApplicationDetailDto Application { get; init; }

    public required string PlaintextSecret { get; init; }
}
