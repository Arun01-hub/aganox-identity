using Aganox.IdentityServer.Application.Abstractions;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aganox.IdentityServer.Infrastructure.Cors;

public sealed class DynamicSpaCorsPolicyProvider(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    CorsOriginCache corsOriginCache) : ICorsPolicyProvider
{
    private readonly object _sync = new();
    private int _cachedVersion = -1;
    private CorsPolicy? _cachedPolicy;

    public async Task<CorsPolicy?> GetPolicyAsync(HttpContext context, string? policyName)
    {
        if (!string.Equals(policyName, "SpaClients", StringComparison.Ordinal))
        {
            return null;
        }

        var version = corsOriginCache.Version;
        if (_cachedPolicy is not null && _cachedVersion == version)
        {
            return _cachedPolicy;
        }

        await using var scope = scopeFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IApplicationRepository>();

        var configuredOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:4200"];
        var registryOrigins = await repository.GetActiveSpaOriginsAsync(context.RequestAborted);

        var origins = configuredOrigins
            .Concat(registryOrigins)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var policy = new CorsPolicyBuilder()
            .WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .Build();

        lock (_sync)
        {
            _cachedVersion = version;
            _cachedPolicy = policy;
        }

        return policy;
    }
}
