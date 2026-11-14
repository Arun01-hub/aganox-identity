namespace Aganox.IdentityServer.Application.Abstractions;

public interface ICorsOriginCache
{
    void Invalidate();
}
