using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;

namespace ATMS.Project.Data.Criteria.History;

public sealed class HistoryOfEntityCriteria(HistoryEntityTypeEnum entityType, Guid entityId) : ACriteria<HistoryEntry>
{
    public override IQueryable<HistoryEntry> Apply(IQueryable<HistoryEntry> query)
    {
        var type = (int)entityType;
        return query.Where(entry => entry.EntityType == type && entry.EntityId == entityId);
    }
}
