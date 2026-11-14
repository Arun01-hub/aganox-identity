using Aganox.IdentityServer.Application.Abstractions;
using Aganox.IdentityServer.Application.Options;
using Aganox.IdentityServer.Application.Services;
using Aganox.IdentityServer.Application.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aganox.IdentityServer.Application.DependencyInjection;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ApplicationRegistryOptions>(
            configuration.GetSection(ApplicationRegistryOptions.SectionName));

        services.AddScoped<IApplicationRegistrationService, ApplicationRegistrationService>();
        services.AddSingleton<ClientIdGenerator>();
        services.AddScoped<ApplicationRegistrationValidator>();
        services.AddScoped<RedirectUriValidator>();

        return services;
    }
}
