using ATMS.Data.Enums;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Enums;
using ATMS.Project.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Project.Data.Repositories;

public class WorkProjectInvitationRepository(ProjectDbContext context) : IWorkProjectInvitationRepository
{
    // The validator's checks alone let two invitations at 19 participants both pass, or the same email
    // be invited twice at once. The checks and the insert run here under a lock on the project's row: a
    // second invitation to the same project waits for the first to commit, then sees it. Other projects
    // are not held up. Whatever else the caller has added to the context — the outbox message — is saved
    // in the same commit.
    public async Task<WorkProjectInvitationRefusal?> AddWithinLimitAsync(
        WorkProjectInvitation invitation,
        int limit,
        CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT 1 FROM \"Projects\" WHERE \"Id\" = {invitation.WorkProjectId} FOR UPDATE",
            cancellationToken);

        var invitations = LivePending(invitation.WorkProjectId);
        if (await invitations.AnyAsync(x => x.NormalizedEmail == invitation.NormalizedEmail, cancellationToken))
        {
            return WorkProjectInvitationRefusal.AlreadyInvited;
        }

        var participants = await context.WorkProjectParticipants
            .CountAsync(x => x.WorkProjectId == invitation.WorkProjectId, cancellationToken);
        if (participants + await invitations.CountAsync(cancellationToken) >= limit)
        {
            return WorkProjectInvitationRefusal.LimitReached;
        }

        await context.WorkProjectInvitations.AddAsync(invitation, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return null;
    }

    public Task<List<WorkProjectInvitation>> GetLivePendingAsync(
        Guid workProjectId,
        CancellationToken cancellationToken)
    {
        return LivePending(workProjectId)
            .AsNoTracking()
            .Include(x => x.Role)
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

    // Admin retries an invitation for about 16 hours. One still unanswered after 24 is dead: it is not
    // shown, takes no place in the project and does not stop the same email from being invited again.
    // Nothing deletes it — the row simply stops counting.
    private IQueryable<WorkProjectInvitation> LivePending(Guid workProjectId)
    {
        var aliveSince = DateTime.UtcNow.AddHours(-24);

        return context.WorkProjectInvitations
            .Where(x => x.WorkProjectId == workProjectId &&
                        x.Status == (int)WorkProjectInvitationStatusEnum.Pending &&
                        x.CreatedAt > aliveSince);
    }
}
