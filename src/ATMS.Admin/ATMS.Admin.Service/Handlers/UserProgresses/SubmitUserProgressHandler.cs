using ATMS.Admin.Contracts.Commands.UserProgresses;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Interfaces;
using ATMS.Contracts.Events.Users;
using ATMS.Messaging.Configuration;
using ATMS.Messaging.Interfaces;
using MediatR;

namespace ATMS.Admin.Service.Handlers.UserProgresses;

public class SubmitUserProgressHandler(
    ICurrentUser currentUser,
    IUserRepository userRepository,
    IUserProgressRepository userProgressRepository,
    IMessagePublisher messagePublisher) : IRequestHandler<SubmitUserProgressCommand>
{
    public async Task Handle(SubmitUserProgressCommand command, CancellationToken cancellationToken)
    {
        var progress = await userProgressRepository.FindAsync(p => p.UserId == currentUser.Id, cancellationToken)
                       ?? throw new AuthException(AuthErrorType.InvalidToken, AuthMessages.InvalidToken);

        var user = await userRepository.FindAsync(u => u.Id == currentUser.Id, cancellationToken)
                   ?? throw new AuthException(AuthErrorType.InvalidToken, AuthMessages.InvalidToken);
        
        var personalInfo = progress.PersonalInfo!;
        var organizationId = progress.OrganizationId;
        var invitedUsers = progress.InvitedUsers?.ToList() ?? [];
        
        await userProgressRepository.SubmitAsync(progress, user, cancellationToken);
        
        var @event = new UserUpdatedEvent(
            currentUser.Id,
            personalInfo.Name,
            personalInfo.Surname,
            personalInfo.AvatarPath);

        await messagePublisher.PublishAsync(
            MessagingConstants.Exchanges.UserEvents,
            MessagingConstants.RoutingKeys.UserUpdated,
            @event,
            cancellationToken);

        foreach (var invitedUser in invitedUsers)
        {
            var inviteEvent = new UserInvitedEvent(
                invitedUser.Email,
                invitedUser.Name,
                invitedUser.Surname,
                organizationId,
                currentUser.Id);
            
            await messagePublisher.PublishAsync(
                MessagingConstants.Exchanges.UserEvents,
                MessagingConstants.RoutingKeys.UserInvited,
                inviteEvent,
                cancellationToken);
        }
    }
}