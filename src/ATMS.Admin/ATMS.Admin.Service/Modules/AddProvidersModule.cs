using ATMS.Admin.Service.Providers;
using ATMS.Admin.Service.Providers.Interfaces;
using ATMS.Application.Exceptions.Configuration;
using ATMS.Application.Exceptions.Resources;
using ATMS.Application.Infrastructure;
using ATMS.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Admin.Service.Modules;

public static class AddProvidersModule
{
    public static IServiceCollection AddProviderServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient("ProjectClient", client =>
        {
            var providerOptions = configuration.GetSection(nameof(ProviderOptions)).Get<ProviderOptions>()
                                  ?? throw new ConfigurationException(ConfigurationErrorType.ProviderSectionNotFound,
                                      string.Format(ExceptionMessages.ConfigSectionNotFound, nameof(ProviderOptions)));

            
            client.BaseAddress = new Uri(providerOptions.ProjectServiceUrl);
            
            client.Timeout = TimeSpan.FromSeconds(providerOptions.TimeoutSeconds);

            client.DefaultRequestHeaders.Add("User-Agent", "ATMS.Admin.API");
        }).AddHttpMessageHandler<AuthorizationDelegatingHandler>();

        services.AddScoped<IOrganizationProvider, OrganizationsProvider>();

        return services;
    }
}