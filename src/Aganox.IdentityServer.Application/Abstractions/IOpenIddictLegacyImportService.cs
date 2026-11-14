namespace Aganox.IdentityServer.Application.Abstractions;

public interface IOpenIddictLegacyImportService
{
    Task ImportMissingApplicationsAsync(CancellationToken cancellationToken = default);
}
