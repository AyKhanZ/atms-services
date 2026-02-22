using ATMS.Exceptions.Configuration;
using ATMS.Infrastructure.Options.Interfaces;

namespace ATMS.Infrastructure.Options;

public class QueueOptions : IOptions
{
    public string Host { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Host))
            throw new ConfigurationException(ConfigurationErrorType.Queue_HostNotFound, "QueueOptions: 'Host' is required.");

        if (string.IsNullOrWhiteSpace(Username))
            throw new ConfigurationException(ConfigurationErrorType.Queue_UserNameNotFound, "QueueOptions: 'Username' is required.");

        if (string.IsNullOrWhiteSpace(Password))
            throw new ConfigurationException(ConfigurationErrorType.Queue_PasswordNotFound, "QueueOptions: 'Password' is required.");
    }
}
