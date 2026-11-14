using Aganox.IdentityServer.Application.Abstractions;
using Aganox.IdentityServer.Domain.Entities;
using Aganox.IdentityServer.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;

namespace Aganox.IdentityServer.Infrastructure.Seeding;

public sealed class OpenIddictSeeder(
    IServiceProvider serviceProvider,
    ILogger<OpenIddictSeeder> logger) : IDbSeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var provider = scope.ServiceProvider;

        var context = provider.GetRequiredService<AuthDbContext>();
        var environment = provider.GetRequiredService<IWebHostEnvironment>();
        if (environment.IsEnvironment("Testing"))
        {
            await context.Database.EnsureCreatedAsync(cancellationToken);
        }
        else
        {
            await context.Database.MigrateAsync(cancellationToken);
        }

        await SeedRolesAsync(provider, cancellationToken);
        await SeedUsersAsync(provider, cancellationToken);
        await SeedScopesAsync(provider, cancellationToken);

        if (environment.IsDevelopment())
        {
            var bootstrapper = provider.GetRequiredService<IDevelopmentClientBootstrapper>();
            await bootstrapper.EnsureDevelopmentClientsAsync(cancellationToken);
        }

        var legacyImport = provider.GetRequiredService<IOpenIddictLegacyImportService>();
        await legacyImport.ImportMissingApplicationsAsync(cancellationToken);

        logger.LogInformation("Identity database seeding completed.");
    }

    private static async Task SeedRolesAsync(IServiceProvider provider, CancellationToken cancellationToken)
    {
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var role in new[] { "User", "Administrator" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SeedUsersAsync(IServiceProvider provider, CancellationToken cancellationToken)
    {
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();

        const string email = "admin@aganox.local";
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                DisplayName = "Administrator"
            };

            var result = await userManager.CreateAsync(user, "ChangeMe123!");
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create seed user: {string.Join(", ", result.Errors.Select(error => error.Description))}");
            }
        }

        if (!await userManager.IsInRoleAsync(user, "User"))
        {
            await userManager.AddToRoleAsync(user, "User");
        }

        if (!await userManager.IsInRoleAsync(user, "Administrator"))
        {
            await userManager.AddToRoleAsync(user, "Administrator");
        }
    }

    private static async Task SeedScopesAsync(IServiceProvider provider, CancellationToken cancellationToken)
    {
        var scopeManager = provider.GetRequiredService<IOpenIddictScopeManager>();

        if (await scopeManager.FindByNameAsync("aganox.api") is null)
        {
            await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "aganox.api",
                DisplayName = "Aganox API",
                Resources = { "aganox.api" }
            }, cancellationToken);
        }
    }
}
