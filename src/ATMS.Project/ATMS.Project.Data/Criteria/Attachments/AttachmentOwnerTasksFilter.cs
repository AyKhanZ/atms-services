using ATMS.Data.Criteria;
using ATMS.Project.Data.Entities;

namespace ATMS.Project.Data.Criteria.Attachments;

public sealed class AttachmentOwnerTasksFilter : ACriteria<WorkTask>
{
    public Guid ProjectId { get; init; }

    public Guid? WorkTaskId { get; init; }

    public Guid? ParentWorkTaskId { get; init; }

    public Guid? WorkTicketId { get; init; }

    public override IQueryable<WorkTask> Apply(IQueryable<WorkTask> query)
    {
        query = query.Where(task => task.WorkProjectId == ProjectId);

        if (WorkTaskId.HasValue)
        {
            query = query.Where(task => task.Id == WorkTaskId.Value);
        }

        if (ParentWorkTaskId.HasValue)
        {
            query = query.Where(task => task.ParentWorkTaskId == ParentWorkTaskId.Value);
        }

        if (WorkTicketId.HasValue)
        {
            query = query.Where(task => task.WorkTicketId == WorkTicketId.Value);
        }

        return query;
    }
}
