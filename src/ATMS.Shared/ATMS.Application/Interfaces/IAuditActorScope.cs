namespace ATMS.Application.Interfaces;

public interface IAuditActorScope
{
    void ActAs(Guid userId);
}
