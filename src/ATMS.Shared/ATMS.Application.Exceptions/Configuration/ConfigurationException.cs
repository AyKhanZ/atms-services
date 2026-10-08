using ATMS.Application.Exceptions.Enums;

namespace ATMS.Application.Exceptions.Configuration;

public sealed class ConfigurationException : Exception
{
    public ConfigurationErrorTypeEnum ErrorType { get; }
    public ConfigurationException(ConfigurationErrorTypeEnum errorType, string message)
        : base(message) => ErrorType = errorType;
}
