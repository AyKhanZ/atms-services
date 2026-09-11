using ATMS.Data.Criteria;
using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Project.Data.Criteria.WorkTickets;

public sealed class WorkTicketsByProjectCriteria(Guid projectId, Guid? milestoneId, string? search = null) : ACriteria<WorkTicket>
{
    public override IQueryable<WorkTicket> Apply(IQueryable<WorkTicket> query)
    {
        query = query.Where(ticket => ticket.WorkProjectId == projectId);

        if (milestoneId.HasValue)
        {
            query = query.Where(ticket => ticket.WorkGroupId == milestoneId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
            var pattern = $"%{term}%";
            query = query.Where(ticket =>
                EF.Functions.ILike(ticket.Code, pattern, "\\") ||
                EF.Functions.ILike(ticket.Title, pattern, "\\"));
        }

        return query;
    }
}
