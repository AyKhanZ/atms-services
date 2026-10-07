using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;

namespace ATMS.Project.Data.Criteria.WorkProjectInvitations;

// Admin retries an invitation for about 16 hours. One still unanswered after 24 is dead: it is not
// shown, takes no place in the project and does not stop the same email from being invited again.
// Nothing deletes it — the row simply stops counting. Both the invitation and the participant
// repositories count with this one rule.
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
