using System.Net;
using System.Net.Mail;
using ATMS.Email.Services;
using ATMS.Email.Services.Interfaces;
using ATMS.Application.Exceptions.Configuration;
using ATMS.Application.Exceptions.Resources;
using ATMS.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Email.Modules;

public static class EmailServicesModule
{
    public static IServiceCollection AddEmailServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        var emailOptions = configuration.GetSection(nameof(EmailOptions)).Get<EmailOptions>()
                           ?? throw new ConfigurationException(ConfigurationErrorType.EmailSectionNotFound,
                               string.Format(LogMessages.ConfigSectionNotFound, nameof(EmailOptions)));

        var smtp = new SmtpClient(emailOptions.SmtpServer, emailOptions.Port)
        {
            EnableSsl = emailOptions.EnableSsl,
            Credentials = new NetworkCredential(emailOptions.From, emailOptions.Password)
        };

        services.AddFluentEmail(emailOptions.From, emailOptions.UserName)
            .AddRazorRenderer()
            .AddSmtpSender(smtp);

        services.AddScoped<IEmailSender, EmailSender>();

        return services;
    }
}
