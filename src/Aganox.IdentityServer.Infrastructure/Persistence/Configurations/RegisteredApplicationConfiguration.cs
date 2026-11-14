using Aganox.IdentityServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aganox.IdentityServer.Infrastructure.Persistence.Configurations;

public sealed class RegisteredApplicationConfiguration : IEntityTypeConfiguration<RegisteredApplication>
{
    public void Configure(EntityTypeBuilder<RegisteredApplication> builder)
    {
        builder.ToTable("RegisteredApplications");

        builder.HasKey(application => application.Id);

        builder.Property(application => application.ClientId)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(application => application.ClientId)
            .IsUnique();

        builder.Property(application => application.DisplayName)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(application => application.Description)
            .HasMaxLength(2000);

        builder.Property(application => application.OpenIddictApplicationId)
            .HasMaxLength(450);

        builder.Property(application => application.ConsentType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(application => application.CreatedByUserId)
            .HasMaxLength(450);

        builder.HasIndex(application => new { application.Status, application.Environment });

        builder.HasMany(application => application.RedirectUris)
            .WithOne(uri => uri.RegisteredApplication)
            .HasForeignKey(uri => uri.RegisteredApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(application => application.AllowedOrigins)
            .WithOne(origin => origin.RegisteredApplication)
            .HasForeignKey(origin => origin.RegisteredApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(application => application.Scopes)
            .WithOne(scope => scope.RegisteredApplication)
            .HasForeignKey(scope => scope.RegisteredApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(application => application.Credentials)
            .WithOne(credential => credential.RegisteredApplication)
            .HasForeignKey(credential => credential.RegisteredApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(application => application.AuditEntries)
            .WithOne(entry => entry.RegisteredApplication)
            .HasForeignKey(entry => entry.RegisteredApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
