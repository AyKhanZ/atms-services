using ATMS.Application.Exceptions.Enums;

namespace ATMS.Application.Exceptions.Entity;

public sealed class EntityException : Exception
{
    public EntityErrorTypeEnum ErrorType { get; }
    public EntityException(EntityErrorTypeEnum errorType, string message)
        : base(message) => ErrorType = errorType;
}
