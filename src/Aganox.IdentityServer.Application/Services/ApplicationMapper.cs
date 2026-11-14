using Aganox.IdentityServer.Application.Models.Applications;
using Aganox.IdentityServer.Domain.Entities;
using Aganox.IdentityServer.Domain.Enums;

namespace Aganox.IdentityServer.Application.Services;

public static class ApplicationMapper
{
    public static ApplicationDetailDto ToDetailDto(RegisteredApplication application)
    {
        var primary = application.Credentials
            .Where(credential => credential.IsPrimary && credential.RevokedAt is null)
            .OrderByDescending(credential => credential.CreatedAt)
            .FirstOrDefault();

        return new ApplicationDetailDto
        {
            Id = application.Id,
            ClientId = application.ClientId,
            DisplayName = application.DisplayName,
            Description = application.Description,
            ApplicationType = application.ApplicationType,
            Environment = application.Environment,
            Status = application.Status,
            RedirectUris = application.RedirectUris
                .Where(uri => uri.Kind == RedirectUriKind.Redirect)
                .Select(uri => uri.Uri)
                .ToList(),
            PostLogoutRedirectUris = application.RedirectUris
                .Where(uri => uri.Kind == RedirectUriKind.PostLogout)
                .Select(uri => uri.Uri)
                .ToList(),
            AllowedOrigins = application.AllowedOrigins.Select(origin => origin.Origin).ToList(),
            Scopes = application.Scopes.Select(scope => scope.Scope).ToList(),
            PrimaryCredential = primary is null
                ? null
                : new ClientCredentialSummaryDto
                {
                    Id = primary.Id,
                    SecretPrefix = primary.SecretPrefix,
                    CreatedAt = primary.CreatedAt,
                    RevokedAt = primary.RevokedAt,
                    IsPrimary = primary.IsPrimary
                },
            CreatedAt = application.CreatedAt,
            UpdatedAt = application.UpdatedAt
        };
    }

    public static ApplicationAuditDto ToAuditDto(ApplicationAuditEntry entry) =>
        new()
        {
            Id = entry.Id,
            Action = entry.Action,
            ActorUserId = entry.ActorUserId,
            OccurredAt = entry.OccurredAt,
            Summary = entry.Summary,
            DetailsJson = entry.DetailsJson
        };
}
