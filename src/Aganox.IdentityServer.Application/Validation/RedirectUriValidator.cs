using Aganox.IdentityServer.Application.Options;
using Aganox.IdentityServer.Domain.Enums;
using Microsoft.Extensions.Options;

namespace Aganox.IdentityServer.Application.Validation;

public sealed class RedirectUriValidator(IOptions<ApplicationRegistryOptions> options)
{
    public void ValidateUris(
        DeploymentEnvironment environment,
        IReadOnlyList<string> uris,
        string fieldName)
    {
        foreach (var uriText in uris)
        {
            ValidateSingleUri(environment, uriText, fieldName);
        }
    }

    public void ValidateSingleUri(DeploymentEnvironment environment, string uriText, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(uriText))
        {
            throw new ApplicationValidationException($"{fieldName} cannot be empty.");
        }

        if (uriText.Contains('*', StringComparison.Ordinal))
        {
            throw new ApplicationValidationException($"{fieldName} '{uriText}' must not contain wildcards.");
        }

        if (!Uri.TryCreate(uriText, UriKind.Absolute, out var uri))
        {
            throw new ApplicationValidationException($"{fieldName} '{uriText}' is not a valid absolute URI.");
        }

        if (!string.IsNullOrEmpty(uri.Fragment))
        {
            throw new ApplicationValidationException($"{fieldName} '{uriText}' must not contain a fragment.");
        }

        var settings = options.Value;
        var host = uri.Host;

        if (environment == DeploymentEnvironment.Production)
        {
            if (settings.ProductionRequireHttpsRedirects &&
                !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) &&
                !IsDevelopmentHostException(host, settings))
            {
                throw new ApplicationValidationException(
                    $"{fieldName} '{uriText}' must use HTTPS in Production.");
            }

            if (!IsHostAllowedForProduction(host, settings))
            {
                throw new ApplicationValidationException(
                    $"{fieldName} host '{host}' is not in the production redirect allowlist.");
            }

            return;
        }

        if (string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        throw new ApplicationValidationException(
            $"{fieldName} '{uriText}' must use http or https.");
    }

    public void ValidateOrigins(IReadOnlyList<string> origins)
    {
        foreach (var origin in origins)
        {
            if (string.IsNullOrWhiteSpace(origin))
            {
                throw new ApplicationValidationException("Allowed origin cannot be empty.");
            }

            if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri) ||
                !string.IsNullOrEmpty(uri.PathAndQuery.Trim('/')) ||
                !string.IsNullOrEmpty(uri.Fragment))
            {
                throw new ApplicationValidationException(
                    $"Allowed origin '{origin}' must be an absolute origin (scheme + host + optional port).");
            }
        }
    }

    private static bool IsDevelopmentHostException(string host, ApplicationRegistryOptions settings) =>
        settings.DevelopmentRedirectHostExceptions.Any(exception =>
            string.Equals(exception, host, StringComparison.OrdinalIgnoreCase) ||
            host.EndsWith("." + exception, StringComparison.OrdinalIgnoreCase));

    private static bool IsHostAllowedForProduction(string host, ApplicationRegistryOptions settings)
    {
        if (settings.ProductionAllowedRedirectHosts.Length == 0)
        {
            return false;
        }

        foreach (var allowed in settings.ProductionAllowedRedirectHosts)
        {
            if (allowed.StartsWith('*'))
            {
                var suffix = allowed.TrimStart('*');
                if (host.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            else if (string.Equals(host, allowed, StringComparison.OrdinalIgnoreCase) ||
                     host.EndsWith("." + allowed, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
