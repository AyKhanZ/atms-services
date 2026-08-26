using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Interfaces;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using ATMS.Project.Contracts.Commands.WorkProjects;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using ATMS.Project.Services.Security.Interfaces;
using MediatR;

namespace ATMS.Project.Services.Handlers.WorkProjects;

public class DeleteWorkProjectParticipantHandler(
    ICurrentUser currentUser,
    IWorkProjectRepository workProjectRepository,
    ICacheService cache,
    IProjectPermissionService projectPermissionService)
    : IRequestHandler<DeleteWorkProjectParticipantCommand>
{
    public async Task Handle(DeleteWorkProjectParticipantCommand command, CancellationToken cancellationToken)
    {
        var project = await workProjectRepository.FindAsync(command.ProjectId, cancellationToken);
        if (project is null)
        {
            throw new EntityException(EntityErrorType.NotFound, WorkProjectMessages.NotFound);
        }

        var participant = project.WorkProjectParticipants.FirstOrDefault(x => x.Id == command.ParticipantId);
        if (participant is null)
        {
            throw new EntityException(EntityErrorType.NotFound, WorkProjectMessages.ParticipantNotFound);
        }

        var now = DateTime.UtcNow;
        participant.IsDeleted = true;
        participant.DeletedAt = now;
        participant.DeletedById = currentUser.Id;

        foreach (var role in participant.WorkProjectParticipantRoles)
        {
            role.IsDeleted = true;
            role.DeletedAt = now;
            role.DeletedById = currentUser.Id;
        }
        workProjectRepository.Touch(project);

        await workProjectRepository.SaveAsync(cancellationToken);
        await cache.RemoveAsync(CacheKeys.Project.ProjectById(project.Id), cancellationToken);
        await projectPermissionService.RemoveUserPermissionsAsync(project.Id, participant.UserId, cancellationToken);
    }
}
