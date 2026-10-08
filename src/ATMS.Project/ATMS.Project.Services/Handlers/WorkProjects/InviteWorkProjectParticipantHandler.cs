using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Exceptions.Enums;
using ATMS.Application.Interfaces;
using ATMS.Caching.Services.Interfaces;
using ATMS.Contracts.Events.Users;
using ATMS.Data.Constants;
using ATMS.Data.Enums;
using ATMS.Data.Messaging;
using ATMS.Messaging.Configuration;
using ATMS.Project.Contracts.Commands.WorkProjects;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Enums;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Infrastructure;
using ATMS.Project.Services.Resources;
using ATMS.Project.Services.Validation.WorkProjects;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace ATMS.Project.Services.Handlers.WorkProjects;

public sealed class InviteWorkProjectParticipantHandler(
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
            ?? throw new EntityException(EntityErrorTypeEnum.NotFound, WorkProjectMessages.NotFound);

        var email = command.Email.Trim();
        var name = command.Name.Trim();
        var surname = command.Surname.Trim();

        // admin creates the account, the invitation waits here until project hears about the user
        // the outbox message is saved in the same commit as the invitation, so neither goes alone
        await outboxRepository.AddAsync(
            MessagingConstants.Exchanges.UserEvents,
            MessagingConstants.RoutingKeys.UserInvited,
            new UserInvitedEvent(email, name, surname, project.OrganizationId, currentUser.Id, project.Title),
            cancellationToken);

        var refusal = await invitationRepository.AddWithinLimitAsync(
            new WorkProjectInvitation
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
            },
            WorkProjectParticipantLimit.Max,
            cancellationToken);

        // another invite took the last place (or the same email) after validation
        if (refusal is not null)
        {
            throw new ValidationException(
            [
                new ValidationFailure(
                    nameof(InviteWorkProjectParticipantCommand.Email),
                    refusal == WorkProjectParticipantRefusalEnum.AlreadyInvited
                        ? WorkProjectMessages.InvitationAlreadySent
                        : string.Format(WorkProjectMessages.ParticipantsLimitExceeded, WorkProjectParticipantLimit.Max))
            ]);
        }

        await cache.RemoveWorkProjectAsync(project.Id, cancellationToken);
    }
}
