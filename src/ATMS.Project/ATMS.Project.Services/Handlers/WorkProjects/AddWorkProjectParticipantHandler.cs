using ATMS.Application.Exceptions.Entity;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using ATMS.Project.Contracts.Commands.WorkProjects;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using MediatR;

namespace ATMS.Project.Services.Handlers.WorkProjects;

public class AddWorkProjectParticipantHandler(
    IWorkProjectRepository workProjectRepository,
    ICacheService cache)
    : IRequestHandler<AddWorkProjectParticipantCommand>
{
    public async Task Handle(AddWorkProjectParticipantCommand command, CancellationToken cancellationToken)
    {
        var project = await workProjectRepository.FindAsync(command.ProjectId, cancellationToken);
        if (project is null)
        {
            throw new EntityException(EntityErrorType.NotFound, WorkProjectMessages.NotFound);
        }

        project.WorkProjectParticipants.Add(new WorkProjectParticipant
        {
            Id = Guid.NewGuid(),
            UserId = command.UserId,
            WorkProjectParticipantRoles =
            [
                new WorkProjectParticipantRole
                {
                    Id = Guid.NewGuid(),
                    RoleId = command.RoleId
                }
            ]
        });
        workProjectRepository.Touch(project);

        await workProjectRepository.SaveAsync(cancellationToken);
        await cache.RemoveAsync(CacheKeys.Project.ProjectById(project.Id), cancellationToken);
    }
}
