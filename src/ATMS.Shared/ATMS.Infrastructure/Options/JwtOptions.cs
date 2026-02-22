using ATMS.Exceptions.Configuration;
using ATMS.Infrastructure.Options.Interfaces;

namespace ATMS.Infrastructure.Options;

public class JwtOptions : IOptions
{
    public string Key { get; init; }
    public string Issuer { get; init; }
    public string Audience { get; init; }
    public int TokenExpirationInMinutes { get; init; }
    public int RefreshTokenExpirationInDays { get; init; }
    public int EmailConfirmationTokenExpirationInHours { get; init; }
    public int MaxRefreshTokenLifetimeExpirationInDays { get; init; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Key))
        {
            throw new ConfigurationException(ConfigurationErrorType.JWT_KeyNotFound,
                "JwtOptions:Key not found .");
        }

        if (string.IsNullOrWhiteSpace(Issuer))
        {
            throw new ConfigurationException(ConfigurationErrorType.JWT_IssuerNotFound,
                "JwtOptions:Issuer not found .");
        }

        if (string.IsNullOrWhiteSpace(Audience))
        {
            throw new ConfigurationException(ConfigurationErrorType.JWT_AudienceNotFound,
                "JwtOptions:Audience not found .");
        }

        if (TokenExpirationInMinutes <= 0)
        {
            throw new ConfigurationException(ConfigurationErrorType.JWT_TokenExpirationInMinutesNotFound,
                "JwtOptions:TokenExpirationInMinutes invalid format .");
        }

        if (RefreshTokenExpirationInDays <= 0)
        {
            throw new ConfigurationException(ConfigurationErrorType.JWT_RefreshTokenExpirationInDaysNotFound,
                "JwtOptions:RefreshTokenExpirationInDays invalid format .");
        }

        if (EmailConfirmationTokenExpirationInHours <= 0)
        {
            throw new ConfigurationException(ConfigurationErrorType.JWT_EmailConfirmationTokenExpirationInHoursNotFound,
                "JwtOptions:EmailConfirmationTokenExpirationInHours invalid format .");
        }

        if (MaxRefreshTokenLifetimeExpirationInDays <= 0)
        {
            throw new ConfigurationException(ConfigurationErrorType.JWT_MaxRefreshTokenLifetimeExpirationInDaysNotFound,
                "JwtOptions:MaxRefreshTokenLifetimeExpirationInDays invalid format .");
        }
    }
}
