using Aganox.IdentityServer.Domain.Enums;

namespace Aganox.IdentityServer.Domain.Entities;

public class RegisteredRedirectUri
{
    public Guid Id { get; set; }

    public Guid RegisteredApplicationId { get; set; }

    public RegisteredApplication RegisteredApplication { get; set; } = null!;

    public required string Uri { get; set; }

    public RedirectUriKind Kind { get; set; }
}
