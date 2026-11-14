using System.Text.RegularExpressions;
using Aganox.IdentityServer.Application.Options;
using Aganox.IdentityServer.Domain.Enums;
using Microsoft.Extensions.Options;

namespace Aganox.IdentityServer.Application.Services;

public sealed partial class ClientIdGenerator(IOptions<ApplicationRegistryOptions> options)
{
    public string Generate(string displayName, DeploymentEnvironment environment, string? slug)
    {
        var prefix = options.Value.ClientIdPrefix;
        var normalizedSlug = NormalizeSlug(slug ?? displayName);
        var envSuffix = environment switch
        {
            DeploymentEnvironment.Development => "dev",
            DeploymentEnvironment.Staging => "stg",
            DeploymentEnvironment.Production => "prd",
            _ => "app"
        };

        return $"{prefix}-{normalizedSlug}-{envSuffix}";
    }

    private static string NormalizeSlug(string value)
    {
        var lower = value.Trim().ToLowerInvariant();
        lower = NonAlphaNumeric().Replace(lower, "-");
        lower = MultiDash().Replace(lower, "-").Trim('-');

        if (string.IsNullOrEmpty(lower))
        {
            lower = "app";
        }

        return lower.Length > 40 ? lower[..40].Trim('-') : lower;
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonAlphaNumeric();

    [GeneratedRegex("-{2,}")]
    private static partial Regex MultiDash();
}
