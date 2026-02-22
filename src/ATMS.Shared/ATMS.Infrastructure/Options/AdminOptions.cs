using ATMS.Exceptions.Configuration;
using ATMS.Infrastructure.Options.Interfaces;

namespace ATMS.Infrastructure.Options;

public class AdminOptions : IOptions
{
    public string Email { get; set; }
    public string Password { get; set; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            throw new ConfigurationException(ConfigurationErrorType.Admin_EmailNotFound, "Admin: 'Email' not found .");
        }
        if (string.IsNullOrWhiteSpace(Password))
        {
            throw new ConfigurationException(ConfigurationErrorType.Admin_PasswordNotFound, "Admin: 'Password' not found .");
        }
    }
}
