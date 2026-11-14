namespace Aganox.IdentityServer.Application.Abstractions;

public interface IDevelopmentClientBootstrapper
{
    Task EnsureDevelopmentClientsAsync(CancellationToken cancellationToken = default);
}
