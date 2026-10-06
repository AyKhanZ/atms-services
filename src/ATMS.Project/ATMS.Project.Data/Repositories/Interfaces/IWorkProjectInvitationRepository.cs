using ATMS.Project.Data.Entities;

namespace ATMS.Project.Data.Repositories.Interfaces;

public interface IWorkProjectInvitationRepository
{
    Task AddAsync(WorkProjectInvitation invitation, CancellationToken cancellationToken);

    Task<List<WorkProjectInvitation>> GetLivePendingAsync(Guid workProjectId, CancellationToken cancellationToken);

    Task<List<WorkProjectInvitation>> GetPendingByEmailAsync(string normalizedEmail, CancellationToken cancellationToken);
}
