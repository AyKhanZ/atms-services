using ATMS.Data.Enums;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Project.Data.Repositories;

public class WorkProjectInvitationRepository(ProjectDbContext context) : IWorkProjectInvitationRepository
{
    public async Task AddAsync(WorkProjectInvitation invitation, CancellationToken cancellationToken)
    {
        await context.WorkProjectInvitations.AddAsync(invitation, cancellationToken);
    }

    // Admin retries an invitation for about 16 hours. One still unanswered after 24 is dead: it is not
    // shown, takes no place in the project and does not stop the same email from being invited again.
    // Nothing deletes it — the row simply stops counting.
    public Task<List<WorkProjectInvitation>> GetLivePendingAsync(
        Guid workProjectId,
        CancellationToken cancellationToken)
    {
        var aliveSince = DateTime.UtcNow.AddHours(-24);

        return context.WorkProjectInvitations
            .AsNoTracking()
            .Include(x => x.Role)
            .Where(x => x.WorkProjectId == workProjectId &&
                        x.Status == (int)WorkProjectInvitationStatusEnum.Pending &&
                        x.CreatedAt > aliveSince)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    // Every pending invitation, dead ones too: an answer from Admin that comes late still settles it.
    public Task<List<WorkProjectInvitation>> GetPendingByEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken)
    {
        return context.WorkProjectInvitations
            .Where(x => x.NormalizedEmail == normalizedEmail &&
                        x.Status == (int)WorkProjectInvitationStatusEnum.Pending)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
