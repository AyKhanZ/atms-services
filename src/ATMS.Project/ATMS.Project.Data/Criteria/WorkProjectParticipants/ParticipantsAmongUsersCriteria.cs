using ATMS.Data.Criteria;
using ATMS.Project.Data.Entities;

namespace ATMS.Project.Data.Criteria.WorkProjectParticipants;

// Takes a user query rather than repeating its conditions, so a rule written once for users —
// who is an employee — holds for participants too.
public sealed class ParticipantsAmongUsersCriteria(IQueryable<User> users) : ACriteria<WorkProjectParticipant>
{
    public override IQueryable<WorkProjectParticipant> Apply(IQueryable<WorkProjectParticipant> query)
        => query.Where(participant => users.Any(user => user.Id == participant.UserId));
}
