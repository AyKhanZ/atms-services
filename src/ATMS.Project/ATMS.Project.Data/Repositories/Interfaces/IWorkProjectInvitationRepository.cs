using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Enums;

namespace ATMS.Project.Data.Repositories.Interfaces;

public interface IWorkProjectInvitationRepository
{
    Task<WorkProjectParticipantRefusal?> AddWithinLimitAsync(
        WorkProjectInvitation invitation,
        int limit,
        CancellationToken cancellationToken);

    Task<List<WorkProjectInvitation>> GetLivePendingAsync(Guid workProjectId, CancellationToken cancellationToken);

    Task<List<WorkProjectInvitation>> GetPendingByEmailAsync(string normalizedEmail, CancellationToken cancellationToken);

    Task<WorkProjectInvitation?> FindPendingAsync(Guid workProjectId, Guid invitationId, CancellationToken cancellationToken);

    Task<bool> IsPendingAsync(Guid workProjectId, Guid invitationId, CancellationToken cancellationToken);

    Task SaveAsync(CancellationToken cancellationToken);
}
