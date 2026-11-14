namespace Aganox.IdentityServer.Application.Models.Applications;

public sealed class RegisterApplicationResult
{
    public required ApplicationDetailDto Application { get; init; }

    public string? PlaintextSecret { get; init; }
}
