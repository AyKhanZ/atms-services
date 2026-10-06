using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Interfaces;
using ATMS.Caching.Services.Interfaces;
using ATMS.Contracts.Events.Users;
using ATMS.Data.Constants;
using ATMS.Data.Enums;
using ATMS.Data.Messaging;
using ATMS.Messaging.Configuration;
using ATMS.Project.Contracts.Commands.WorkProjects;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Caching;
using ATMS.Project.Services.Resources;
using MediatR;

namespace ATMS.Project.Services.Handlers.WorkProjects;

public class InviteWorkProjectParticipantHandler(
    ICurrentUser currentUser,
    IWorkProjectRepository workProjectRepository,
    IWorkProjectInvitationRepository invitationRepository,
    IOutboxRepository outboxRepository,
    ICacheService cache)
    : IRequestHandler<InviteWorkProjectParticipantCommand>
{
    public async Task Handle(InviteWorkProjectParticipantCommand command, CancellationToken cancellationToken)
    {
        var project = await workProjectRepository.FindRootAsync(command.ProjectId, cancellationToken)
            ?? throw new EntityException(EntityErrorType.NotFound, WorkProjectMessages.NotFound);

        var email = command.Email.Trim();
        var name = command.Name.Trim();
        var surname = command.Surname.Trim();

        // The account is created by Admin; the invitation waits here and becomes a participant when
        // Project hears about the new user. Both rows leave in one save, so neither is sent alone.
        await invitationRepository.AddAsync(new WorkProjectInvitation
        {
            Id = Guid.NewGuid(),
            WorkProjectId = project.Id,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            Name = name,
            Surname = surname,
            RoleId = RoleIds.OrgClientViewer,
            Status = (int)WorkProjectInvitationStatusEnum.Pending,
            InvitedById = currentUser.Id,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await outboxRepository.AddAsync(
            MessagingConstants.Exchanges.UserEvents,
            MessagingConstants.RoutingKeys.UserInvited,
            new UserInvitedEvent(email, name, surname, project.OrganizationId, currentUser.Id, project.Title),
            cancellationToken);

        await workProjectRepository.SaveAsync(cancellationToken);
        await cache.RemoveWorkProjectAsync(project.Id, cancellationToken);
    }
}
