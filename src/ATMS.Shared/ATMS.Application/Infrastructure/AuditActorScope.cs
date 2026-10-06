using ATMS.Application.Interfaces;
using ATMS.Data.Interfaces;

namespace ATMS.Application.Infrastructure;

// Audit columns and history take the author from the signed-in user. A consumer has no request and no
// user, so one that saves on someone's behalf — a participant joining by invitation — names them here.
public sealed class AuditActorScope(ICurrentUser currentUser) : IAuditActorScope, IAuditActorAccessor
{
    private Guid? _actorId;

    public Guid? UserId => _actorId ?? ((IAuditActorAccessor)currentUser).UserId;

    public void ActAs(Guid userId)
    {
        _actorId = userId;
    }
}
