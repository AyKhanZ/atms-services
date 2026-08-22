namespace ATMS.Data.Interfaces;

public interface IAuditActorAccessor
{
    Guid? UserId { get; }
}
