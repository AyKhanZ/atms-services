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

public class DeleteWorkProjectHandler(
    ICurrentUser currentUser,
    IWorkProjectRepository workProjectRepository,
    ICacheService cache,
    IProjectPermissionService projectPermissionService)
    : IRequestHandler<DeleteWorkProjectCommand>
{
    public async Task Handle(DeleteWorkProjectCommand command, CancellationToken cancellationToken)
    {
        var project = await workProjectRepository.FindAsync(command.Id, cancellationToken);
        if (project is null)
        {
            throw new EntityException(EntityErrorType.NotFound, WorkProjectMessages.NotFound);
        }

        var now = DateTime.UtcNow;
        project.IsDeleted = true;
        project.DeletedAt = now;
        project.DeletedById = currentUser.Id;

        foreach (var participant in project.WorkProjectParticipants)
        {
            participant.IsDeleted = true;
            participant.DeletedAt = now;
            participant.DeletedById = currentUser.Id;

            foreach (var role in participant.WorkProjectParticipantRoles)
            {
                role.IsDeleted = true;
                role.DeletedAt = now;
                role.DeletedById = currentUser.Id;
            }
        }

        await workProjectRepository.SaveAsync(cancellationToken);
        await cache.RemoveAsync(CacheKeys.Project.ProjectById(project.Id), cancellationToken);
        await projectPermissionService.RemoveProjectPermissionsAsync(
            project.Id,
            project.WorkProjectParticipants.Select(participant => participant.UserId),
            cancellationToken);
    }
}
