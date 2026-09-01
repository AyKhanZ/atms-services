using ATMS.Project.Data.DbContexts;
using ATMS.Data.Criteria;
using ATMS.Project.Data.Criteria.WorkTickets;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Project.Data.Repositories;

public class WorkTicketRepository(ProjectDbContext context) : IWorkTicketRepository
{
    public async Task<KeysetPagedResult<WorkTicket>> GetManyAsync(WorkTicketsByProjectCriteria criteria, KeysetPaginationCriteria<WorkTicket> pagination, CancellationToken cancellationToken)
    {
        var query = criteria.Apply(context.WorkTickets
            .AsNoTracking()
            .Where(ticket => ticket.WorkGroup.ParentWorkGroupId != null)
            .Include(ticket => ticket.WorkGroup)
                .ThenInclude(milestone => milestone.ParentWorkGroup)
            .Include(ticket => ticket.WorkTicketType)
                .ThenInclude(type => type.Translations)
            .Include(ticket => ticket.WorkTicketStatus)
                .ThenInclude(status => status.Translations)
            .Include(ticket => ticket.Assignee)
                .ThenInclude(participant => participant.User));

        var items = await pagination
            .Apply(query, ticket => ticket.CreatedAt, ticket => ticket.Id)
            .ToArrayAsync(cancellationToken);

        return pagination.ToResult(items, ticket => ticket.CreatedAt, ticket => ticket.Id);
    }

    public Task<WorkTicket?> GetAsync(Guid projectId, Guid workTicketId, CancellationToken cancellationToken)
    {
        return context.WorkTickets
            .AsNoTracking()
            .Where(ticket => ticket.WorkGroup.ParentWorkGroupId != null)
            .Include(ticket => ticket.WorkGroup)
                .ThenInclude(milestone => milestone.ParentWorkGroup)
            .Include(ticket => ticket.WorkTicketType)
                .ThenInclude(type => type.Translations)
            .Include(ticket => ticket.WorkTicketStatus)
                .ThenInclude(status => status.Translations)
            .Include(ticket => ticket.Priority)
                .ThenInclude(priority => priority.Translations)
            .Include(ticket => ticket.Assignee)
                .ThenInclude(participant => participant.User)
            .AsSplitQuery()
            .FirstOrDefaultAsync(
                ticket => ticket.Id == workTicketId && ticket.WorkProjectId == projectId,
                cancellationToken);
    }

    public Task<WorkTicket?> FindAsync(Guid projectId, Guid workTicketId, CancellationToken cancellationToken)
    {
        return context.WorkTickets.FirstOrDefaultAsync(
            ticket => ticket.Id == workTicketId && ticket.WorkProjectId == projectId,
            cancellationToken);
    }

    public Task<bool> IsMilestoneExistAsync(Guid projectId, Guid milestoneId, CancellationToken cancellationToken)
    {
        return context.WorkGroups.AnyAsync(
            group => group.Id == milestoneId &&
                     group.WorkProjectId == projectId &&
                     group.ParentWorkGroupId != null,
            cancellationToken);
    }

    public Task<bool> IsProjectParticipantExistAsync(Guid projectId, Guid participantId, CancellationToken cancellationToken)
    {
        return context.WorkProjectParticipants.AnyAsync(
            participant => participant.Id == participantId && participant.WorkProjectId == projectId,
            cancellationToken);
    }

    public async Task CreateAsync(WorkTicket workTicket, CancellationToken cancellationToken)
    {
        await context.WorkTickets.AddAsync(workTicket, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return context.SaveChangesAsync(cancellationToken);
    }
}
