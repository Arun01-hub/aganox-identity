using Aganox.IdentityServer.Application.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace Aganox.IdentityServer.Infrastructure.Security;

public sealed class ClientSecretHasher : IClientSecretHasher
{
    private readonly PasswordHasher<string> _hasher = new();

    public string Hash(string plaintext) => _hasher.HashPassword("client-secret", plaintext);

    public bool Verify(string plaintext, string hash) =>
        _hasher.VerifyHashedPassword("client-secret", hash, plaintext) != PasswordVerificationResult.Failed;
}
