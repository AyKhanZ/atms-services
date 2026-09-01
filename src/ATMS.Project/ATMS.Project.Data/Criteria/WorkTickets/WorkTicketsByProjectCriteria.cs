using ATMS.Data.Criteria;
using ATMS.Project.Data.Entities;

namespace ATMS.Project.Data.Criteria.WorkTickets;

public sealed class WorkTicketsByProjectCriteria(Guid projectId, Guid? milestoneId) : ACriteria<WorkTicket>
{
    public override IQueryable<WorkTicket> Apply(IQueryable<WorkTicket> query)
    {
        query = query.Where(ticket => ticket.WorkProjectId == projectId);

        if (milestoneId.HasValue)
        {
            query = query.Where(ticket => ticket.WorkGroupId == milestoneId.Value);
        }

        return query;
    }
}
