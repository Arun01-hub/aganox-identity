using Aganox.IdentityServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aganox.IdentityServer.Infrastructure.Persistence.Configurations;

public sealed class ApplicationAuditEntryConfiguration : IEntityTypeConfiguration<ApplicationAuditEntry>
{
    public void Configure(EntityTypeBuilder<ApplicationAuditEntry> builder)
    {
        builder.ToTable("ApplicationAuditEntries");

        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.Summary)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(entry => entry.DetailsJson)
            .HasMaxLength(8000);

        builder.Property(entry => entry.ActorUserId)
            .HasMaxLength(450);

        builder.HasIndex(entry => new { entry.RegisteredApplicationId, entry.OccurredAt });
    }
}
