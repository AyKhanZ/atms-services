using ATMS.Data.Criteria;
using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Project.Data.Criteria.WorkGroups;

public sealed class MilestonesByProjectCriteria(Guid projectId, string? search) : ACriteria<WorkGroup>
{
    public override IQueryable<WorkGroup> Apply(IQueryable<WorkGroup> query)
    {
        query = query.Where(group =>
            group.WorkProjectId == projectId &&
            group.ParentWorkGroupId != null);

        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        var pattern = $"%{search.Trim()}%";
        return query.Where(group =>
            EF.Functions.ILike(group.Title, pattern) ||
            (group.ParentWorkGroup != null && EF.Functions.ILike(group.ParentWorkGroup.Title, pattern)));
    }
}
