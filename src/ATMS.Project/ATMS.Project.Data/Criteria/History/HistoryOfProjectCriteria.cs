using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;

namespace ATMS.Project.Data.Criteria.History;

// Groups and milestones have no page of their own, so their changes belong to the project's history.
public sealed class HistoryOfProjectCriteria(Guid projectId) : ACriteria<HistoryEntry>
{
    public override IQueryable<HistoryEntry> Apply(IQueryable<HistoryEntry> query) =>
        query.Where(entry =>
            entry.WorkProjectId == projectId &&
            (entry.EntityType == (int)HistoryEntityTypeEnum.Project ||
             entry.EntityType == (int)HistoryEntityTypeEnum.WorkGroup ||
             entry.EntityType == (int)HistoryEntityTypeEnum.Milestone));
}
