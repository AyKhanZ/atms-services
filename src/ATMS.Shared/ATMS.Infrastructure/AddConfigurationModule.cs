using ATMS.Exceptions.Configuration;
using ATMS.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Infrastructure;

public static class AddConfigurationModule
{
    public static IServiceCollection AddConfigurations(this IServiceCollection services, IConfiguration configuration)
    {
        var adminDatabaseOptions = configuration.GetSection(nameof(AdminDatabaseOptions)).Get<AdminDatabaseOptions>() 
            ?? throw new ConfigurationException(ConfigurationErrorType.Database_SectionNotFound,
            $"Configuration for section '{nameof(AdminDatabaseOptions)}' is not found or could not be loaded.");

        var adminOptions = configuration.GetSection(nameof(AdminOptions)).Get<AdminOptions>() 
            ?? throw new ConfigurationException(ConfigurationErrorType.Admin_SectionNotFound,
            $"Configuration for section '{nameof(AdminOptions)}' is not found or could not be loaded.");

        var emailOptions = configuration.GetSection(nameof(EmailOptions)).Get<EmailOptions>()
            ?? throw new ConfigurationException(ConfigurationErrorType.Email_SectionNotFound,
            $"Configuration for section '{nameof(AdminOptions)}' is not found or could not be loaded.");

        var imagesOptions = configuration.GetSection(nameof(ImagesOptions)).Get<ImagesOptions>()
            ?? throw new ConfigurationException(ConfigurationErrorType.Images_SectionNotFound,
            $"Configuration for section '{nameof(AdminOptions)}' is not found or could not be loaded.");

        var jwtOptions = configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>()
            ?? throw new ConfigurationException(ConfigurationErrorType.JWT_SectionNotFound,
            $"Configuration for section '{nameof(AdminOptions)}' is not found or could not be loaded.");

        var queueOptions = configuration.GetSection(nameof(QueueOptions)).Get<QueueOptions>()
            ?? throw new ConfigurationException(ConfigurationErrorType.Queue_SectionNotFound,
            $"Configuration for section '{nameof(AdminOptions)}' is not found or could not be loaded.");

        var resirectUrlOptions = configuration.GetSection(nameof(RedirectUrlOptions)).Get<RedirectUrlOptions>()
            ?? throw new ConfigurationException(ConfigurationErrorType.RedirectUrl_SectionNotFound,
            $"Configuration for section '{nameof(AdminOptions)}' is not found or could not be loaded.");

        adminDatabaseOptions.Validate();
        adminOptions.Validate();
        emailOptions.Validate();
        imagesOptions.Validate();
        jwtOptions.Validate();
        queueOptions.Validate();
        resirectUrlOptions.Validate();

        services.AddSingleton(adminDatabaseOptions);
        services.AddSingleton(adminOptions);
        services.AddSingleton(emailOptions);
        services.AddSingleton(imagesOptions);
        services.AddSingleton(jwtOptions);
        services.AddSingleton(queueOptions);
        services.AddSingleton(resirectUrlOptions);

        return services;
    }
}
