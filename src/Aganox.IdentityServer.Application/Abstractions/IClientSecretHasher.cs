namespace Aganox.IdentityServer.Application.Abstractions;

public interface IClientSecretHasher
{
    string Hash(string plaintext);

    bool Verify(string plaintext, string hash);
}
