
using ATMS.Application.Exceptions.Enums;

namespace ATMS.Application.Exceptions.Image;

public sealed class ImageException(
    ImageErrorTypeEnum errorType,
    string userMessage,
    string logMessage,
    string propertyName = "Image") : Exception(logMessage)
{
    public ImageErrorTypeEnum ErrorType { get; } = errorType;
    public string UserMessage { get; } = userMessage;
    public string PropertyName { get; } = propertyName;
}
