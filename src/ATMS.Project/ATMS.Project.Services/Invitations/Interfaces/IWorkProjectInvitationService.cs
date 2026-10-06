using ATMS.Project.Data.Entities;

namespace ATMS.Project.Services.Invitations.Interfaces;

public interface IWorkProjectInvitationService
{
    Task SettlePendingAsync(User user, CancellationToken cancellationToken);
}
