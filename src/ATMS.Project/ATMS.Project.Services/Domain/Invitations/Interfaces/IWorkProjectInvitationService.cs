using ATMS.Project.Data.Entities;

namespace ATMS.Project.Services.Domain.Invitations.Interfaces;

public interface IWorkProjectInvitationService
{
    Task SettlePendingAsync(User user, CancellationToken cancellationToken);
}
