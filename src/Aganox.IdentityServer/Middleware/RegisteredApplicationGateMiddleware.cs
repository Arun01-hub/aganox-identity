using Aganox.IdentityServer.Application.Abstractions;
using Aganox.IdentityServer.Domain.Enums;

namespace Aganox.IdentityServer.Middleware;

public sealed class RegisteredApplicationGateMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IApplicationRepository repository)
    {
        if (IsOidcClientRequest(context.Request))
        {
            var clientId = await ResolveClientIdAsync(context.Request);
            if (!string.IsNullOrEmpty(clientId))
            {
                var application = await repository.GetByClientIdAsync(clientId, context.RequestAborted);
                if (application is not null && application.Status != ApplicationStatus.Active)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "invalid_client",
                        error_description = "The client application is not active."
                    });
                    return;
                }
            }
        }

        await next(context);
    }

    private static bool IsOidcClientRequest(HttpRequest request)
    {
        var path = request.Path.Value ?? string.Empty;
        return path.StartsWith("/connect/authorize", StringComparison.OrdinalIgnoreCase) ||
               path.StartsWith("/connect/token", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<string?> ResolveClientIdAsync(HttpRequest request)
    {
        if (request.Query.TryGetValue("client_id", out var queryClientId) &&
            !string.IsNullOrWhiteSpace(queryClientId))
        {
            return queryClientId.ToString();
        }

        if (request.HasFormContentType)
        {
            var form = await request.ReadFormAsync();
            if (form.TryGetValue("client_id", out var formClientId) &&
                !string.IsNullOrWhiteSpace(formClientId))
            {
                return formClientId.ToString();
            }
        }

        return null;
    }
}
