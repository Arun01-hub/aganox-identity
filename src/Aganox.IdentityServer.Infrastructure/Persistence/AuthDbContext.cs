using Aganox.IdentityServer.Domain.Entities;
using Aganox.IdentityServer.Infrastructure.Persistence.Configurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Aganox.IdentityServer.Infrastructure.Persistence;

public class AuthDbContext : IdentityDbContext<ApplicationUser>
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options)
    {
    }

    public DbSet<RegisteredApplication> RegisteredApplications => Set<RegisteredApplication>();

    public DbSet<RegisteredRedirectUri> RegisteredRedirectUris => Set<RegisteredRedirectUri>();

    public DbSet<RegisteredAllowedOrigin> RegisteredAllowedOrigins => Set<RegisteredAllowedOrigin>();

    public DbSet<RegisteredApplicationScope> RegisteredApplicationScopes => Set<RegisteredApplicationScope>();

    public DbSet<ClientCredential> ClientCredentials => Set<ClientCredential>();

    public DbSet<ApplicationAuditEntry> ApplicationAuditEntries => Set<ApplicationAuditEntry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.UseOpenIddict();

        builder.ApplyConfiguration(new RegisteredApplicationConfiguration());
        builder.ApplyConfiguration(new RegisteredRedirectUriConfiguration());
        builder.ApplyConfiguration(new RegisteredAllowedOriginConfiguration());
        builder.ApplyConfiguration(new RegisteredApplicationScopeConfiguration());
        builder.ApplyConfiguration(new ClientCredentialConfiguration());
        builder.ApplyConfiguration(new ApplicationAuditEntryConfiguration());
    }
}
