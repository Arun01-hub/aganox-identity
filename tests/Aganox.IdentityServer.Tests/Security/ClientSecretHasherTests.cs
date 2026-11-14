using Aganox.IdentityServer.Infrastructure.Security;

namespace Aganox.IdentityServer.Tests.Security;

public sealed class ClientSecretHasherTests
{
    [Fact]
    public void Hash_and_verify_roundtrip()
    {
        var hasher = new ClientSecretHasher();
        var hash = hasher.Hash("super-secret-value");
        Assert.DoesNotContain("super-secret", hash);
        Assert.True(hasher.Verify("super-secret-value", hash));
        Assert.False(hasher.Verify("wrong", hash));
    }
}
