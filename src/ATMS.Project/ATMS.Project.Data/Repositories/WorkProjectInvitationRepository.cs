using ATMS.Data.Enums;
using ATMS.Project.Data.Criteria.WorkProjectInvitations;
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
    public async Task<WorkProjectParticipantRefusal?> AddWithinLimitAsync(
        WorkProjectInvitation invitation,
        int limit,
        CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT 1 FROM \"Projects\" WHERE \"Id\" = {invitation.WorkProjectId} FOR UPDATE",
            cancellationToken);

        var invitations = new LivePendingInvitationsCriteria(invitation.WorkProjectId)
            .Apply(context.WorkProjectInvitations);
        if (await invitations.AnyAsync(x => x.NormalizedEmail == invitation.NormalizedEmail, cancellationToken))
        {
            return WorkProjectParticipantRefusal.AlreadyInvited;
        }

        var participants = await context.WorkProjectParticipants
            .CountAsync(x => x.WorkProjectId == invitation.WorkProjectId, cancellationToken);
        if (participants + await invitations.CountAsync(cancellationToken) >= limit)
        {
            return WorkProjectParticipantRefusal.LimitReached;
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
        return new LivePendingInvitationsCriteria(workProjectId)
            .Apply(context.WorkProjectInvitations)
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

    public Task<WorkProjectInvitation?> FindPendingAsync(
        Guid workProjectId,
        Guid invitationId,
        CancellationToken cancellationToken)
    {
        return Pending(workProjectId, invitationId).FirstOrDefaultAsync(cancellationToken);
    }

    public Task<bool> IsPendingAsync(Guid workProjectId, Guid invitationId, CancellationToken cancellationToken)
    {
        return Pending(workProjectId, invitationId).AnyAsync(cancellationToken);
    }

    public Task SaveAsync(CancellationToken cancellationToken)
    {
        return context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<WorkProjectInvitation> Pending(Guid workProjectId, Guid invitationId)
    {
        return context.WorkProjectInvitations.Where(x =>
            x.Id == invitationId &&
            x.WorkProjectId == workProjectId &&
            x.Status == (int)WorkProjectInvitationStatusEnum.Pending);
    }
}
