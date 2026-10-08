using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Exceptions.Enums;
using ATMS.Caching.Services.Interfaces;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Commands.WorkProjects;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Infrastructure;
using ATMS.Project.Services.Resources;
using MediatR;

namespace ATMS.Project.Services.Handlers.WorkProjects;

public sealed class CancelWorkProjectInvitationHandler(
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
                         ?? throw new EntityException(EntityErrorTypeEnum.NotFound, WorkProjectMessages.InvitationNotPending);

        // kept as Cancelled: the consumer settles only pending ones, so an account created later won't join
        invitation.Status = (int)WorkProjectInvitationStatusEnum.Cancelled;
        invitation.ProcessedAt = DateTime.UtcNow;

        await invitationRepository.SaveAsync(cancellationToken);
        await cache.RemoveWorkProjectAsync(command.ProjectId, cancellationToken);
    }
}
