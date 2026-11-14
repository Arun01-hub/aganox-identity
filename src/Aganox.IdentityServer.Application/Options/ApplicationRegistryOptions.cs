namespace Aganox.IdentityServer.Application.Options;

public sealed class ApplicationRegistryOptions
{
    public const string SectionName = "ApplicationRegistry";

    public string ClientIdPrefix { get; set; } = "aganox";

    public bool AllowExplicitClientId { get; set; }

    public bool ProductionRequireHttpsRedirects { get; set; } = true;

    public string[] ProductionAllowedRedirectHosts { get; set; } = [];

    public string[] DevelopmentRedirectHostExceptions { get; set; } =
        ["localhost", "127.0.0.1", "rental.aganox.local"];
}
