namespace ATMS.Data.Criteria;

public sealed class CriteriaException(string propertyName, string userMessage) : Exception(userMessage)
{
    public string PropertyName { get; } = propertyName;
    public string UserMessage { get; } = userMessage;
}
