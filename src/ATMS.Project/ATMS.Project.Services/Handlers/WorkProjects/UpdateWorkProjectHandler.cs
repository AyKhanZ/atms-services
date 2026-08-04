using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Interfaces;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using ATMS.Project.Contracts.Commands.WorkProjects;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using AutoMapper;
using MediatR;

namespace ATMS.Project.Services.Handlers.WorkProjects;

public class UpdateWorkProjectHandler(
    ICurrentUser currentUser,
    IMapper mapper,
    IWorkProjectRepository workProjectRepository,
    ICacheService cache)
    : IRequestHandler<UpdateWorkProjectCommand>
{
    public async Task Handle(UpdateWorkProjectCommand command, CancellationToken cancellationToken)
    {
        var project = await workProjectRepository.FindAsync(command.Id, cancellationToken);
        if (project is null)
        {
            throw new EntityException(EntityErrorType.NotFound, WorkProjectMessages.NotFound);
        }

        mapper.Map(command, project);
        project.Title = command.Title.Trim();
        SynchronizeParticipants(project, command.Participants);

        await workProjectRepository.SaveAsync(cancellationToken);
        await cache.RemoveAsync(CacheKeys.Project.ProjectById(project.Id), cancellationToken);
    }

    private void SynchronizeParticipants(
        WorkProject project,
        WorkProjectParticipantCommand[] commands)
    {
        var commandByUser = commands.ToDictionary(x => x.UserId);
        var existingByUser = project.WorkProjectParticipants.ToDictionary(x => x.UserId);
        var now = DateTime.UtcNow;

        foreach (var participant in project.WorkProjectParticipants.Where(x => !commandByUser.ContainsKey(x.UserId)))
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

        foreach (var command in commands)
        {
            if (!existingByUser.TryGetValue(command.UserId, out var participant))
            {
                project.WorkProjectParticipants.Add(CreateParticipant(command));
                continue;
            }

            var currentRole = participant.WorkProjectParticipantRoles.Single();
            if (currentRole.RoleId == command.RoleId)
            {
                continue;
            }

            currentRole.IsDeleted = true;
            currentRole.DeletedAt = now;
            currentRole.DeletedById = currentUser.Id;
            participant.WorkProjectParticipantRoles.Add(new WorkProjectParticipantRole
            {
                Id = Guid.NewGuid(),
                RoleId = command.RoleId
            });
        }
    }

    private WorkProjectParticipant CreateParticipant(WorkProjectParticipantCommand command)
    {
        var participant = new WorkProjectParticipant
        {
            Id = Guid.NewGuid(),
            UserId = command.UserId
        };

        participant.WorkProjectParticipantRoles.Add(new WorkProjectParticipantRole
        {
            Id = Guid.NewGuid(),
            RoleId = command.RoleId
        });

        return participant;
    }

}
