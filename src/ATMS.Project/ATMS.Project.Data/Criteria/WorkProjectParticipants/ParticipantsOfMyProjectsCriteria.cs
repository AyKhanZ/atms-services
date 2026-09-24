using ATMS.Data.Criteria;
using ATMS.Project.Data.Entities;

namespace ATMS.Project.Data.Criteria.WorkProjectParticipants;

public sealed class ParticipantsOfMyProjectsCriteria(Guid userId) : ACriteria<WorkProjectParticipant>
{
    public override IQueryable<WorkProjectParticipant> Apply(IQueryable<WorkProjectParticipant> query)
        => query.Where(participant => participant.WorkProject.WorkProjectParticipants
            .Any(member => member.UserId == userId));
}
