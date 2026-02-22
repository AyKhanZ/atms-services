using ATMS.Exceptions.Configuration;
using ATMS.Infrastructure.Options.Interfaces;

namespace ATMS.Infrastructure.Options;

public class RedirectUrlOptions : IOptions
{
    public string BasePath { get; set; }
    public string ResetPasswordPage { get; set; }
    public string EmailConfirmedPage { get; set; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(BasePath))
            throw new ConfigurationException(ConfigurationErrorType.RedirectUrl_BasePath,
                "RedirectUrlOptions: 'BasePath' not found.");

        if (string.IsNullOrWhiteSpace(ResetPasswordPage))
            throw new ConfigurationException(ConfigurationErrorType.RedirectUrl_ResetPasswordPage,
                "RedirectUrlOptions: 'ResetPasswordPage' not found.");

        if (string.IsNullOrWhiteSpace(EmailConfirmedPage))
            throw new ConfigurationException(ConfigurationErrorType.RedirectUrl_EmailConfirmedPage,
                "RedirectUrlOptions: 'EmailConfirmedPage' not found.");
    }
}
