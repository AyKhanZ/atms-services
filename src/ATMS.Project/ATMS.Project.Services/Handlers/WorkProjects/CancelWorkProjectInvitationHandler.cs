using ATMS.Application.Exceptions.Entity;
using ATMS.Caching.Services.Interfaces;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Commands.WorkProjects;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Caching;
using ATMS.Project.Services.Resources;
using MediatR;

namespace ATMS.Project.Services.Handlers.WorkProjects;

public class CancelWorkProjectInvitationHandler(
    IWorkProjectInvitationRepository invitationRepository,
    ICacheService cache)
    : IRequestHandler<CancelWorkProjectInvitationCommand>
{
    public async Task Handle(CancelWorkProjectInvitationCommand command, CancellationToken cancellationToken)
    {
        var invitation = await invitationRepository.FindPendingAsync(
                             command.ProjectId,
                             command.InvitationId,
                             cancellationToken)
                         ?? throw new EntityException(EntityErrorType.NotFound, WorkProjectMessages.InvitationNotPending);

        // Kept, not deleted: the row says the invitation was withdrawn. The consumer settles only pending
        // ones, so an account Admin creates after this does not join the project.
        invitation.Status = (int)WorkProjectInvitationStatusEnum.Cancelled;
        invitation.ProcessedAt = DateTime.UtcNow;

        await invitationRepository.SaveAsync(cancellationToken);
        await cache.RemoveWorkProjectAsync(command.ProjectId, cancellationToken);
    }
}
