namespace ATMS.Exceptions.Entity;

public class EntityException : Exception
{
    public EntityErrorType EntityErrorType { get; }
    public EntityException(EntityErrorType entityErrorType, string message) : base(message)
    {
        EntityErrorType = entityErrorType;
    }
}
