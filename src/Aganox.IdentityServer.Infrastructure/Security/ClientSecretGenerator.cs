using System.Security.Cryptography;
using Aganox.IdentityServer.Application.Abstractions;

namespace Aganox.IdentityServer.Infrastructure.Security;

public sealed class ClientSecretGenerator : IClientSecretGenerator
{
    public string Generate() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
