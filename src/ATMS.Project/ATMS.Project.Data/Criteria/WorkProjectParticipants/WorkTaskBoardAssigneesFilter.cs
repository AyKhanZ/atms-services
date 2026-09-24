using ATMS.Data.Criteria;
using ATMS.Project.Data.Entities;

namespace ATMS.Project.Data.Criteria.WorkProjectParticipants;

public class WorkTaskBoardAssigneesFilter : ACriteria<WorkProjectParticipant>
{
    public IReadOnlyCollection<Guid> ProjectIds { get; init; } = [];

    public override IQueryable<WorkProjectParticipant> Apply(IQueryable<WorkProjectParticipant> query)
    {
        if (ProjectIds.Count == 0)
        {
            return query;
        }

        var projectIds = ProjectIds;
        return query.Where(participant => projectIds.Contains(participant.WorkProjectId));
    }
}
