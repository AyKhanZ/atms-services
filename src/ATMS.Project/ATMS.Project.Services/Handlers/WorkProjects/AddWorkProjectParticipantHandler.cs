using ATMS.Application.Exceptions.Entity;
using ATMS.Caching.Services.Interfaces;
using ATMS.Project.Contracts.Commands.WorkProjects;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Enums;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using ATMS.Project.Services.Caching;
using ATMS.Project.Services.Notifications.Interfaces;
using ATMS.Project.Services.Security.Interfaces;
using ATMS.Project.Services.Validation.WorkProjects;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace ATMS.Project.Services.Handlers.WorkProjects;

public class AddWorkProjectParticipantHandler(
    IWorkProjectRepository workProjectRepository,
    ICacheService cache,
    IProjectPermissionService projectPermissionService,
    IWorkProjectNotificationService notifications)
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
            UserId = command.UserId,
            WorkProjectParticipantRoles =
            [
                new WorkProjectParticipantRole
                {
                    RoleId = command.RoleId
                }
            ]
        });
        workProjectRepository.Touch(project);

        await notifications.NotifyParticipantsAddedAsync(project, [command.UserId], cancellationToken);

        var refusal = await workProjectRepository.SaveParticipantWithinLimitAsync(
            project.Id,
            command.UserId,
            WorkProjectParticipantLimit.Max,
            cancellationToken);
        if (refusal is not null)
        {
            throw new ValidationException(
            [
                new ValidationFailure(
                    nameof(AddWorkProjectParticipantCommand.UserId),
                    refusal == WorkProjectParticipantRefusal.AlreadyParticipant
                        ? WorkProjectMessages.DuplicateParticipant
                        : string.Format(WorkProjectMessages.ParticipantsLimitExceeded, WorkProjectParticipantLimit.Max))
            ]);
        }

        await cache.RemoveWorkProjectAsync(project.Id, cancellationToken);
        await projectPermissionService.RemoveUserPermissionsAsync(project.Id, command.UserId, cancellationToken);
    }
}
