namespace ATMS.Application.Exceptions.Image;

public class ImageException(
    ImageErrorType errorType,
    string userMessage,
    string logMessage,
    string propertyName = "Image") : Exception(logMessage)
{
    public ImageErrorType ErrorType { get; } = errorType;
    public string UserMessage { get; } = userMessage;
    public string PropertyName { get; } = propertyName;
}
