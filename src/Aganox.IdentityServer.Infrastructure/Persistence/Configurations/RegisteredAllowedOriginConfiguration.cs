using Aganox.IdentityServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aganox.IdentityServer.Infrastructure.Persistence.Configurations;

public sealed class RegisteredAllowedOriginConfiguration : IEntityTypeConfiguration<RegisteredAllowedOrigin>
{
    public void Configure(EntityTypeBuilder<RegisteredAllowedOrigin> builder)
    {
        builder.ToTable("RegisteredAllowedOrigins");

        builder.HasKey(origin => origin.Id);

        builder.Property(origin => origin.Origin)
            .HasMaxLength(500)
            .IsRequired();

        builder.HasIndex(origin => new { origin.RegisteredApplicationId, origin.Origin })
            .IsUnique();
    }
}
