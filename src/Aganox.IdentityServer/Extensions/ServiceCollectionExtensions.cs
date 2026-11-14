using Aganox.IdentityServer.Application.DependencyInjection;
using Aganox.IdentityServer.Application.Abstractions;
using Aganox.IdentityServer.Infrastructure.DependencyInjection;
using Aganox.IdentityServer.Services;

namespace Aganox.IdentityServer.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityServerHost(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllersWithViews();
        services.AddRazorPages();

        services.AddApplication(configuration);
        services.AddInfrastructure(configuration);
        services.AddScoped<IUserAccountService, UserAccountService>();

        services.AddAuthorization(options =>
        {
            options.AddPolicy(Domain.Constants.PolicyNames.RequireAuthenticatedUser, policy =>
                policy.RequireAuthenticatedUser());

            options.AddPolicy(Domain.Constants.PolicyNames.RequireAdministrator, policy =>
                policy.RequireRole("Administrator"));
        });

        services.AddCors();

        return services;
    }
}
