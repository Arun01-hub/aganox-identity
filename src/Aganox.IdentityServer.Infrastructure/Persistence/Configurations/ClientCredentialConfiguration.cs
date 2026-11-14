using Aganox.IdentityServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aganox.IdentityServer.Infrastructure.Persistence.Configurations;

public sealed class ClientCredentialConfiguration : IEntityTypeConfiguration<ClientCredential>
{
    public void Configure(EntityTypeBuilder<ClientCredential> builder)
    {
        builder.ToTable("ClientCredentials");

        builder.HasKey(credential => credential.Id);

        builder.Property(credential => credential.SecretHash)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(credential => credential.SecretPrefix)
            .HasMaxLength(8);

        builder.Property(credential => credential.CreatedByUserId)
            .HasMaxLength(450);

        builder.Property(credential => credential.RevokedByUserId)
            .HasMaxLength(450);

        builder.HasIndex(credential => new { credential.RegisteredApplicationId, credential.IsPrimary });
    }
}
