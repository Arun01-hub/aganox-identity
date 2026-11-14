using Aganox.IdentityServer.Application.Options;
using Aganox.IdentityServer.Application.Validation;
using Aganox.IdentityServer.Domain.Enums;
using Microsoft.Extensions.Options;

namespace Aganox.IdentityServer.Tests.Validation;

public sealed class RedirectUriValidatorTests
{
    private readonly RedirectUriValidator _validator = new(Options.Create(new ApplicationRegistryOptions
    {
        ProductionRequireHttpsRedirects = true,
        ProductionAllowedRedirectHosts = ["aganox.com", "rental.aganox.com"],
        DevelopmentRedirectHostExceptions = ["localhost", "127.0.0.1", "rental.aganox.local"]
    }));

    [Fact]
    public void Production_allows_https_allowlisted_host()
    {
        _validator.ValidateSingleUri(
            DeploymentEnvironment.Production,
            "https://rental.aganox.com/callback",
            "Redirect URI");
    }

    [Fact]
    public void Production_rejects_http_non_exception_host()
    {
        Assert.Throws<ApplicationValidationException>(() =>
            _validator.ValidateSingleUri(
                DeploymentEnvironment.Production,
                "http://evil.example/callback",
                "Redirect URI"));
    }

    [Fact]
    public void Development_allows_localhost_http()
    {
        _validator.ValidateSingleUri(
            DeploymentEnvironment.Development,
            "http://localhost:4200/",
            "Redirect URI");
    }
}
