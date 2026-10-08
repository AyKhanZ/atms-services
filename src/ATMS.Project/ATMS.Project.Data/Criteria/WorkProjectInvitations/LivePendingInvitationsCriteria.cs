using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;

namespace ATMS.Project.Data.Criteria.WorkProjectInvitations;

// an invite unanswered for 24h is dead: hidden, takes no place, the email can be invited again
public sealed class LivePendingInvitationsCriteria(Guid workProjectId) : ACriteria<WorkProjectInvitation>
{
    public override IQueryable<WorkProjectInvitation> Apply(IQueryable<WorkProjectInvitation> query)
    {
        var aliveSince = DateTime.UtcNow.AddHours(-24);

        return query.Where(x => x.WorkProjectId == workProjectId &&
                                x.Status == (int)WorkProjectInvitationStatusEnum.Pending &&
                                x.CreatedAt > aliveSince);
    }
}
