using Aganox.IdentityServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aganox.IdentityServer.Infrastructure.Persistence.Configurations;

public sealed class RegisteredRedirectUriConfiguration : IEntityTypeConfiguration<RegisteredRedirectUri>
{
    public void Configure(EntityTypeBuilder<RegisteredRedirectUri> builder)
    {
        builder.ToTable("RegisteredRedirectUris");

        builder.HasKey(uri => uri.Id);

        builder.Property(uri => uri.Uri)
            .HasMaxLength(2000)
            .IsRequired();

        builder.HasIndex(uri => new { uri.RegisteredApplicationId, uri.Kind, uri.Uri })
            .IsUnique();
    }
}
