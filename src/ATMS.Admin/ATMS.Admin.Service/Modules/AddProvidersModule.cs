using ATMS.Admin.Service.Providers;
using ATMS.Admin.Service.Providers.Interfaces;
using ATMS.Application.Exceptions.Configuration;
using ATMS.Application.Exceptions.Enums;
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
        var providerOptions = configuration.GetSection(nameof(ProviderOptions)).Get<ProviderOptions>()
                              ?? throw new ConfigurationException(ConfigurationErrorTypeEnum.ProviderSectionNotFound,
                                  string.Format(LogMessages.ConfigSectionNotFound, nameof(ProviderOptions)));

        services.AddHttpClient("ProjectClient", client =>
        {
            client.BaseAddress = new Uri(providerOptions.ProjectServiceUrl);
            client.Timeout = TimeSpan.FromSeconds(providerOptions.TimeoutSeconds);
            client.DefaultRequestHeaders.Add("User-Agent", "ATMS.Admin.API");
        }).AddHttpMessageHandler<AuthorizationDelegatingHandler>();

        // Migrations outlast the 5-second ProjectClient timeout.
        services.AddHttpClient(MigrationsProvider.HttpClientName, client =>
        {
            client.BaseAddress = new Uri(providerOptions.ProjectServiceUrl);
            client.Timeout = TimeSpan.FromMinutes(5);
            client.DefaultRequestHeaders.Add("User-Agent", "ATMS.Admin.API");
        }).AddHttpMessageHandler<AuthorizationDelegatingHandler>();

        services.AddScoped<IOrganizationProvider, OrganizationsProvider>();
        services.AddScoped<IMigrationsProvider, MigrationsProvider>();

        return services;
    }
}