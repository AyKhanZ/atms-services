namespace ATMS.Application.Exceptions.Entity;

public class EntityException : Exception
{
    public EntityErrorType ErrorType { get; }
    public EntityException(EntityErrorType errorType, string message)
        : base(message) => ErrorType = errorType;
}
