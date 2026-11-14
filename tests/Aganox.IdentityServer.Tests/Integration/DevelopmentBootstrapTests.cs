using Aganox.IdentityServer.Application.Abstractions;
using Aganox.IdentityServer.Application.Models.Applications;
using Microsoft.Extensions.DependencyInjection;

namespace Aganox.IdentityServer.Tests.Integration;

[Collection("IdentityIntegration")]
public sealed class DevelopmentBootstrapTests(IdentityWebApplicationFactory factory)
{
    [Fact]
    public async Task Development_bootstrap_registers_aganox_rental_in_registry()
    {
        using var scope = factory.Services.CreateScope();
        var bootstrapper = scope.ServiceProvider.GetRequiredService<IDevelopmentClientBootstrapper>();
        await bootstrapper.EnsureDevelopmentClientsAsync();

        var repository = scope.ServiceProvider.GetRequiredService<IApplicationRepository>();
        var rental = await repository.GetByClientIdAsync("aganox-rental");
        Assert.NotNull(rental);
        Assert.Equal(Aganox.IdentityServer.Domain.Enums.ApplicationStatus.Active, rental.Status);
        Assert.Contains(
            rental.RedirectUris,
            uri => uri.Uri.Contains("localhost:4200", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task UpdateRedirectUris_succeeds_after_bootstrap_replaced_child_rows()
    {
        using var scope = factory.Services.CreateScope();
        var bootstrapper = scope.ServiceProvider.GetRequiredService<IDevelopmentClientBootstrapper>();
        await bootstrapper.EnsureDevelopmentClientsAsync();

        var repository = scope.ServiceProvider.GetRequiredService<IApplicationRepository>();
        var registrationService = scope.ServiceProvider.GetRequiredService<IApplicationRegistrationService>();
        var rental = await repository.GetByClientIdAsync("aganox-rental");
        Assert.NotNull(rental);

        var actor = new ActorContext(null);
        await registrationService.UpdateRedirectUrisAsync(
            rental.Id,
            ["http://localhost:4200/admin-callback"],
            ["http://localhost:4200/"],
            actor);

        var updated = await registrationService.GetAsync(rental.Id);
        Assert.NotNull(updated);
        Assert.Contains("http://localhost:4200/admin-callback", updated.RedirectUris);
    }
}
