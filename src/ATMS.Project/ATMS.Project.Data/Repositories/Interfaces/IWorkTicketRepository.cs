using ATMS.Project.Data.Entities;
using ATMS.Data.Criteria;
using ATMS.Project.Data.Criteria.WorkTickets;

namespace ATMS.Project.Data.Repositories.Interfaces;

public interface IWorkTicketRepository
{
    Task<KeysetPagedResult<WorkTicket>> GetManyAsync(
        WorkTicketsByProjectCriteria criteria,
        KeysetPaginationCriteria<WorkTicket> pagination,
        CancellationToken cancellationToken);

    Task<WorkTicket?> GetAsync(Guid projectId, Guid workTicketId, CancellationToken cancellationToken);

    Task<WorkTicket?> FindAsync(Guid projectId, Guid workTicketId, CancellationToken cancellationToken);

    Task<bool> IsMilestoneExistAsync(Guid projectId, Guid milestoneId, CancellationToken cancellationToken);

    Task<bool> IsProjectParticipantExistAsync(Guid projectId, Guid participantId, CancellationToken cancellationToken);

    Task CreateAsync(WorkTicket workTicket, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
