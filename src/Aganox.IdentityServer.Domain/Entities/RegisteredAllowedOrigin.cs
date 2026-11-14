namespace Aganox.IdentityServer.Domain.Entities;

public class RegisteredAllowedOrigin
{
    public Guid Id { get; set; }

    public Guid RegisteredApplicationId { get; set; }

    public RegisteredApplication RegisteredApplication { get; set; } = null!;

    public required string Origin { get; set; }
}
