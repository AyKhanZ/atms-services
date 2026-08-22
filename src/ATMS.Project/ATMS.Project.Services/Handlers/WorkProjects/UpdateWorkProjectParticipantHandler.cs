using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Interfaces;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using ATMS.Project.Contracts.Commands.WorkProjects;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using MediatR;

namespace ATMS.Project.Services.Handlers.WorkProjects;

public class UpdateWorkProjectParticipantHandler(
    ICurrentUser currentUser,
    IWorkProjectRepository workProjectRepository,
    ICacheService cache)
    : IRequestHandler<UpdateWorkProjectParticipantCommand>
{
    public async Task Handle(UpdateWorkProjectParticipantCommand command, CancellationToken cancellationToken)
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

        var currentRole = participant.WorkProjectParticipantRoles.Single();
        if (currentRole.RoleId == command.RoleId)
        {
            return;
        }

        var now = DateTime.UtcNow;
        currentRole.IsDeleted = true;
        currentRole.DeletedAt = now;
        currentRole.DeletedById = currentUser.Id;
        participant.WorkProjectParticipantRoles.Add(new WorkProjectParticipantRole
        {
            Id = Guid.NewGuid(),
            RoleId = command.RoleId
        });
        workProjectRepository.Touch(project);

        await workProjectRepository.SaveAsync(cancellationToken);
        await cache.RemoveAsync(CacheKeys.Project.ProjectById(project.Id), cancellationToken);
    }
}
