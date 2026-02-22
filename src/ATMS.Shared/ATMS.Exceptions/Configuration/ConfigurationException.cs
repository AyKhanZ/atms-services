namespace ATMS.Exceptions.Configuration;

public class ConfigurationException : Exception
{
    public ConfigurationErrorType ErrorType { get; }
    public ConfigurationException(ConfigurationErrorType errorType, string message)
        : base(message) => ErrorType = errorType;
}
