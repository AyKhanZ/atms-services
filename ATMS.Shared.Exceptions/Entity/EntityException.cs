namespace ATMS.Shared.Exceptions.Entity;

public class EntityException : Exception
{
    public EntityErrorType EntityErrorType { get; set; }
    public EntityException(EntityErrorType entityErrorType, string message) :base(message)
    {
        EntityErrorType = entityErrorType;
    }
}
