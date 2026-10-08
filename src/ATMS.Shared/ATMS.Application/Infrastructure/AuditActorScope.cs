using ATMS.Application.Interfaces;
using ATMS.Data.Interfaces;

namespace ATMS.Application.Infrastructure;

// a consumer has no signed-in user, so when it saves on someone's behalf it sets the author here
public sealed class AuditActorScope(ICurrentUser currentUser) : IAuditActorScope, IAuditActorAccessor
{
    private Guid? _actorId;

    public Guid? UserId => _actorId ?? ((IAuditActorAccessor)currentUser).UserId;

    public void ActAs(Guid userId)
    {
        _actorId = userId;
    }
}
