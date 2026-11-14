using Aganox.IdentityServer.Domain.Entities;

namespace Aganox.IdentityServer.Application.Abstractions;

public interface IIdentityProviderProvisioner
{
    Task ProvisionAsync(RegisteredApplication application, string? plaintextSecret, CancellationToken cancellationToken = default);

    Task UpdateAsync(RegisteredApplication application, string? plaintextSecret, CancellationToken cancellationToken = default);

    Task ApplyStatusAsync(RegisteredApplication application, CancellationToken cancellationToken = default);

    Task RevokeTokensAsync(RegisteredApplication application, CancellationToken cancellationToken = default);
}
