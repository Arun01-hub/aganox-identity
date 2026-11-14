namespace Aganox.IdentityServer.Application.Abstractions;

public interface IClientSecretGenerator
{
    string Generate();
}
