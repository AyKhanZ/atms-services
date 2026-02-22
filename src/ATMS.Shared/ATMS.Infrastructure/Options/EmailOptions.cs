using ATMS.Exceptions.Configuration;
using ATMS.Infrastructure.Options.Interfaces;

namespace ATMS.Infrastructure.Options;

public class EmailOptions : IOptions
{
    public string From { get; set; }
    public string SmtpServer { get; set; }
    public int Port { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(From))
            throw new ConfigurationException(ConfigurationErrorType.Email_FromNotFound,
                "EmailOptions: 'From' not found.");
        if (string.IsNullOrWhiteSpace(SmtpServer))
            throw new ConfigurationException(ConfigurationErrorType.Email_SmtpServerNotFound,
                "EmailOptions: 'SmtpServer' not found.");
        if (Port <= 0)
            throw new ConfigurationException(ConfigurationErrorType.Email_PortNotFound,
                "EmailOptions: 'Port' must be a positive integer.");
        if (string.IsNullOrWhiteSpace(UserName))
            throw new ConfigurationException(ConfigurationErrorType.Email_UserNameNotFound,
                "EmailOptions: 'UserName' not found.");
        if (string.IsNullOrWhiteSpace(Password))
            throw new ConfigurationException(ConfigurationErrorType.Email_PasswordNotFound,
                "EmailOptions: 'Password' not found.");
    }
}
