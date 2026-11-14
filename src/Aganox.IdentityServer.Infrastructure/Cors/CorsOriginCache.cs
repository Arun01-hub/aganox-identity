using Aganox.IdentityServer.Application.Abstractions;

namespace Aganox.IdentityServer.Infrastructure.Cors;

public sealed class CorsOriginCache : ICorsOriginCache
{
    private int _version;

    public int Version => Volatile.Read(ref _version);

    public void Invalidate() => Interlocked.Increment(ref _version);
}
