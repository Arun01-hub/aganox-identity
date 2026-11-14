using Aganox.IdentityServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aganox.IdentityServer.Infrastructure.Persistence.Configurations;

public sealed class RegisteredApplicationScopeConfiguration : IEntityTypeConfiguration<RegisteredApplicationScope>
{
    public void Configure(EntityTypeBuilder<RegisteredApplicationScope> builder)
    {
        builder.ToTable("RegisteredApplicationScopes");

        builder.HasKey(scope => scope.Id);

        builder.Property(scope => scope.Scope)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(scope => new { scope.RegisteredApplicationId, scope.Scope })
            .IsUnique();
    }
}
